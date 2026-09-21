using System.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using SharedKernel.Extensions;
using SharedKernel.ValueObjects;

using Shared.Extensions;
using Shared.Helpers;
using Shared.Layout;
using Shared.Services;

using Google.Protobuf;

using Domain.Interfaces.Biometrics;

using Domain.Abstractions;

using R = Domain.Entities.Role;
using T = Domain.Entities.User;
using Theme = SharedKernel.ValueObjects.Theme;

namespace Shared.Components
{
    public partial class Avatar : DataComponentBase
    {
        [Inject] public NavigationManager nav { get; set; } = default!;
        [Inject] public AuthServiceClient authClient { get; set; } = default!;
        [Inject] public AuthenticationStateProvider auth { get; set; } = default!;
        [Inject] public ISettingsManager settings { get; set; } = default!;
        [Inject] public IJSRuntime js { get; set; } = default!;
        [Inject] public Microsoft.Extensions.Configuration.IConfiguration configuration { get; set; } = default!;
        [Inject] public Microsoft.Extensions.Logging.ILogger<Avatar> Logger { get; set; } = default!;

        string? mode;
        Language[] languages = [Language.Uz, Language.Ru, Language.En];
        string? language;
        List<R> Roles { get; set; } = new();
        string roleId;
        Permission permission;
        int ProgressPercent = 0;

        int? color;
        public string? userIdStr;
        public static bool isEditDialogOpen = false;
        public static User? profile;
        public bool isFaceIdConfigured = false;

        public string FormModelIdString
        {
            get => FormModel?.Id?.ToString() ?? string.Empty;
            set { }
        }
        public string FormModelPhone
        {
            get => FormModel?.Phone ?? string.Empty;
            set { if (FormModel != null) FormModel.Phone = value; }
        }

        public string FormModelEmail
        {
            get => FormModel?.Email ?? string.Empty;
            set { if (FormModel != null) FormModel.Email = value; }
        }

        public string FormModelFirstName
        {
            get => FormModel?.Name?.FirstName ?? string.Empty;
            set { if (FormModel != null) { FormModel.Name ??= new SharedKernel.Protos.Name(); FormModel.Name.FirstName = value; } }
        }

        public string FormModelLastName
        {
            get => FormModel?.Name?.LastName ?? string.Empty;
            set { if (FormModel != null) { FormModel.Name ??= new SharedKernel.Protos.Name(); FormModel.Name.LastName = value; } }
        }

        public string FormModelMiddleName
        {
            get => FormModel?.Name?.MiddleName ?? string.Empty;
            set { if (FormModel != null) { FormModel.Name ??= new SharedKernel.Protos.Name(); FormModel.Name.MiddleName = value; } }
        }

        public string FormModelAddressCountry
        {
            get => FormModel?.Address?.Country ?? string.Empty;
            set { if (FormModel != null) { FormModel.Address ??= new SharedKernel.Protos.Address(); FormModel.Address.Country = value; } }
        }

        public string FormModelAddressRegion
        {
            get => FormModel?.Address?.Region ?? string.Empty;
            set { if (FormModel != null) { FormModel.Address ??= new SharedKernel.Protos.Address(); FormModel.Address.Region = value; } }
        }

        public string FormModelAddressCity
        {
            get => FormModel?.Address?.City ?? string.Empty;
            set { if (FormModel != null) { FormModel.Address ??= new SharedKernel.Protos.Address(); FormModel.Address.City = value; } }
        }

        public string FormModelAddressDistrict
        {
            get => FormModel?.Address?.District ?? string.Empty;
            set { if (FormModel != null) { FormModel.Address ??= new SharedKernel.Protos.Address(); FormModel.Address.District = value; } }
        }

        public string FormModelAddressStreet
        {
            get => FormModel?.Address?.Street ?? string.Empty;
            set { if (FormModel != null) { FormModel.Address ??= new SharedKernel.Protos.Address(); FormModel.Address.Street = value; } }
        }

        public string FormModelAddressHouse
        {
            get => FormModel?.Address?.House ?? string.Empty;
            set { if (FormModel != null) { FormModel.Address ??= new SharedKernel.Protos.Address(); FormModel.Address.House = value; } }
        }

        public string FormModelAddressApartment
        {
            get => FormModel?.Address?.Apartment ?? string.Empty;
            set { if (FormModel != null) { FormModel.Address ??= new SharedKernel.Protos.Address(); FormModel.Address.Apartment = value; } }
        }

        public string AccentHex => FormModel != null ? Shared.Layout.Color.ToHex(FormModel.Accent) : "#00ffb3";

        public async Task SetLanguage(Language lang)
        {
            if (FormModel != null)
            {
                FormModel.Language = lang;
                await settings.SetLanguageAsync(lang);
                StateHasChanged();
            }
        }

        public async Task SetTheme(Theme theme)
        {
            if (FormModel != null)
            {
                FormModel.Theme = theme;
                await settings.SetThemeAsync(theme);
                await OnThemeChanged(theme.ToString());
            }
        }

        public async Task SetAccent(int accentVal)
        {
            if (FormModel != null)
            {
                FormModel.Accent = accentVal;
                await ChangeTheme(accentVal.ToString());
            }
        }

        public async Task SetAccentHex(string? hex)
        {
            if (!string.IsNullOrWhiteSpace(hex))
            {
                await ChangeTheme(hex);
            }
        }

        public void RemovePhoto()
        {
            if (FormModel != null)
            {
                FormModel.Image = new Domain.Entities.File();
                StateHasChanged();
            }
        }

        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
        [Parameter] public EventCallback OnStateHasChanged { get; set; }
        [SupplyParameterFromForm] public T? FormModel { get; set; } = default!;
        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public EventCallback<EventArgs> OnClose { get; set; }

        [Inject] NavigationManager Nav { get; set; } = default!;
        [Inject] IStringLocalizer L { get; set; } = default!;
        [Inject] ISettingsManager Settings { get; set; } = default!;


        private List<Domain.Entities.File> _imageAttachments = new();

        private void SyncImageAttachments()
        {
            _imageAttachments.Clear();
            if (FormModel?.Image != null && (!string.IsNullOrEmpty(FormModel.Image.Url) || (FormModel.Image.Content != null && FormModel.Image.Content.Length > 0)))
            {
                _imageAttachments.Add(FormModel.Image);
            }
        }

        private void HandleImageChanged()
        {
            if (_imageAttachments.Any())
            {
                FormModel.Image = _imageAttachments.Last();
            }
            else
            {
                FormModel.Image = new Domain.Entities.File();
            }
            StateHasChanged();
        }

        async Task OpenProfile(MouseEventArgs args)
        {
            IsOpen = !IsOpen;
            Logger.LogInformation("[Profile] Opening profile dialog. IsOpen: {IsOpen}", IsOpen);
            try
            {
                userIdStr = await Settings.GetUserIdAsync();
                Logger.LogInformation("[Profile] Current user ID from settings: {UserId}", userIdStr ?? "(empty)");
                profile ??= await ClientFactory.GetClient<T>().Read(new() { Id = Guid.TryParse(userIdStr, out var uG) ? uG : null });

                if (profile == null)
                {
                    Logger.LogWarning("[Profile] Profile entity could not be retrieved. Clearing access token and refreshing.");
                    await Settings.SetAccessTokenAsync(string.Empty);
                    Nav.Refresh(true);
                    return;
                }

                Logger.LogInformation("[Profile] Profile loaded successfully. Email: {Email}, Phone: {Phone}, Theme: {Theme}",
                    profile.Email, profile.Phone, profile.Theme);

                FormModel = profile.Clone();
                FormModel.Name ??= new SharedKernel.Protos.Name();
                FormModel.Address ??= new SharedKernel.Protos.Address();
                FormModel.Image ??= new Domain.Entities.File();
                SyncImageAttachments();
                StateHasChanged();
                await Settings.SetLanguageAsync(profile.Language);
                await Settings.SetThemeAsync(profile.Theme);
                await Settings.SetAccentAsync(profile.Accent);

                await CheckFaceIdStatusAsync();

                await OnStateHasChanged.InvokeAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "[Profile] Error loading user profile");
                Dialog.Window = b => b.AddContent(1, e.Message);
            }
            await OnStateHasChanged.InvokeAsync();
        }
        protected override async Task OnInitializedAsync()
        {
            var lang = FormModel?.Language;
            var theme = FormModel?.Theme;
            color = await settings.GetAccentAsync();
            languages = languages.OrderBy(l => l == lang ? 0 : 1).ToArray();
            permission = await settings.GetPermissionAsync();
            await CheckFaceIdStatusAsync();
        }

        private async Task CheckFaceIdStatusAsync()
        {
            try
            {
                var currentUserId = userIdStr;
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    currentUserId = await Settings.GetUserIdAsync();
                }
                if (string.IsNullOrWhiteSpace(currentUserId) && auth != null)
                {
                    var authId = await auth.GetUserIdAsync();
                    if (authId != Guid.Empty) currentUserId = authId.ToString();
                }
                if (string.IsNullOrWhiteSpace(currentUserId) && FormModel?.Id != null && FormModel.Id.Value != Guid.Empty)
                {
                    currentUserId = FormModel.Id.ToString();
                }

                if (!string.IsNullOrWhiteSpace(currentUserId) && FaceIdService != null)
                {
                    isFaceIdConfigured = await FaceIdService.HasFaceRegisteredAsync(currentUserId);
                    Logger.LogInformation("[Profile] Checked Face ID status for user '{UserId}': Configured={Configured}", currentUserId, isFaceIdConfigured);
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "[Profile] Error checking Face ID status.");
            }
        }

        async Task ChangeMode()
        {
            var theme = await Settings.GetThemeAsync();
            var newTheme = theme == Theme.Light ? Theme.Dark : Theme.Light;
            await Settings.SetThemeAsync(newTheme);
            try
            {
                if (profile is not null)
                {
                    profile.Theme = newTheme;
                }
                await ClientFactory.GetClient<T>().Update(profile);
            }
            catch (Exception)
            {
                await Settings.SetThemeAsync(theme);
            }
        }

        public void ChangeLanguage(ChangeEventArgs args)
        {
            var lang = args.Value?.ToString();
            if (Enum.TryParse<Language>(lang, true, out var language))
            {
                FormModel!.Language = language;
            }
            StateHasChanged();
        }


        public async Task ChangeTheme(string? newColor)
        {
            int parsedColor = 65459;
            if (!string.IsNullOrWhiteSpace(newColor))
            {
                var s = newColor.Trim();
                if (s.StartsWith("#"))
                {
                    if (int.TryParse(s.TrimStart('#'), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var hVal))
                        parsedColor = hVal;
                }
                else if (int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var dVal))
                {
                    parsedColor = dVal;
                }
                else if (int.TryParse(s, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var rhVal))
                {
                    parsedColor = rhVal;
                }
            }

            if (parsedColor <= 3)
            {
                parsedColor = parsedColor switch
                {
                    0 => 65459,   // #00ffb3
                    1 => 3899638, // #3b82f6
                    2 => 15680580,// #ef4444
                    3 => 15381256,// #eab308
                    _ => 65459
                };
            }

            color = parsedColor & 0xFFFFFF;

            if (FormModel is not null)
            {
                FormModel.Accent = color.Value;
            }

            await settings.SetAccentAsync(color.Value);

            var theme = await settings.GetThemeAsync();

            bool isDark = false;
            var module = await js.InvokeAsync<IJSObjectReference>("import", "./_content/Shared/Layout/Color.razor.js");
            if (theme == Theme.System)
            {
                isDark = await module.InvokeAsync<bool>("isSystemDark");
            }
            else
            {
                isDark = theme == Theme.Dark;
            }

            var hexColor = Shared.Layout.Color.ToHex(color.Value);
            await module.InvokeAsync<string>("applyTheme", hexColor, isDark);
        }

        private string ThemeString
        {
            get => FormModel?.Theme.ToString() ?? "System";
            set
            {
                if (Enum.TryParse<Theme>(value, true, out var theme) && FormModel is not null)
                {
                    FormModel.Theme = theme;
                }
            }
        }

        private List<Material.Web.Components.Select.SelectOption> ThemeOptions => new()
        {
            new Material.Web.Components.Select.SelectOption("System", l?["System"] ?? "System"),
            new Material.Web.Components.Select.SelectOption("Light", l?["Light"] ?? "Light"),
            new Material.Web.Components.Select.SelectOption("Dark", l?["Dark"] ?? "Dark")
        };

        public async Task OnThemeChanged(string newThemeStr)
        {
            if (Enum.TryParse<Theme>(newThemeStr, true, out var newTheme))
            {
                if (FormModel is not null)
                {
                    FormModel.Theme = newTheme;
                }
                await settings.SetThemeAsync(newTheme);

                bool isDark = false;
                var module = await js.InvokeAsync<IJSObjectReference>("import", "./_content/Shared/Layout/Color.razor.js");
                if (newTheme == Theme.System)
                {
                    isDark = await module.InvokeAsync<bool>("isSystemDark");
                }
                else
                {
                    isDark = newTheme == Theme.Dark;
                }

                var hexColor = Shared.Layout.Color.ToHex(color ?? 65459);
                await module.InvokeAsync<string>("applyTheme", hexColor, isDark);

                StateHasChanged();
            }
        }

        async Task GetRoles()
        {
            roleId = (await auth.GetRoleIdAsync()).ToString();

            if (Roles.Any()) return;

            Roles = await ClientFactory.GetClient<R>().Read(new());
        }


        long maxFileSize = long.MaxValue;


        public async Task Switch(string id)
        {
            var oldRole = roleId;
            roleId = id;
            var access = await authClient.SwitchAsync(new() { RoleId = id });
            if (access != null)
            {
                await settings.SetAccessTokenAsync(access.Token);
                permission = await settings.GetPermissionAsync();
            }
            else
                roleId = oldRole;

            StateHasChanged();
        }


        public async Task UpdateProfile()
        {
            if (FormModel is null) return;
            Logger.LogInformation("[Profile] Updating profile for user {UserId}. Email: {Email}, Phone: {Phone}", FormModel.Id, FormModel.Email, FormModel.Phone);

            if (_imageAttachments.Any())
            {
                FormModel.Image = _imageAttachments.Last();
            }
            try
            {
                var update = await ClientFactory.GetClient<T>().Update(FormModel);

                if (update is not null)
                {
                    FormModel = update;
                    profile = update;
                    SyncImageAttachments();
                    Logger.LogInformation("[Profile] Profile updated successfully for user {UserId}.", update.Id);
                }

                if (FormModel is not null)
                {
                    await settings.SetThemeAsync(FormModel.Theme);
                    await settings.SetLanguageAsync(FormModel.Language);
                    await settings.SetAccentAsync(FormModel.Accent);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Profile] Failed to update profile for user {UserId}", FormModel.Id);
            }

            await Dialog.Close();
            StateHasChanged();
        }
        async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            if (FormModel is null)
                return;

            var bytes = await ReadBytesAsync(e.File, maxFileSize);
            FormModel.Image ??= new Domain.Entities.File();
            FormModel.Image.Content = bytes;
            FormModel.Image.FileName = e.File.Name;
            FormModel.Image.ContentType = e.File.ContentType ?? "application/octet-stream";
        }

        private static async Task<byte[]> ReadBytesAsync(IBrowserFile file, long maxBytes)
        {
            await using var stream = file.OpenReadStream(maxBytes);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }

        private SharedKernel.Protos.Address GetOrCreateAddress()
        {
            if (FormModel == null) return new SharedKernel.Protos.Address();
            FormModel.Address ??= new SharedKernel.Protos.Address();
            return FormModel.Address;
        }

        [Inject] public IFaceIdService? FaceIdService { get; set; }

        public bool isFaceIdModalOpen = false;
        public string faceIdStatusMessage = "Пожалуйста, смотрите прямо в камеру...";
        public string faceIdStatusColor = "var(--md-sys-color-primary)";
        public int currentEnrollmentStage = 1;
        private bool _isFaceProcessingFrame = false;
        private string? _enrollmentSessionId = null;
        [Inject] public Domain.Interfaces.Hardware.ICameraStreamerUiProvider? CameraStreamerUiProvider { get; set; }

        private Dictionary<string, object> CameraStreamerParameters => new()
        {
            { "Width", 280 },
            { "Height", 280 },
            { "OnFrame", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<string>(this, OnFaceIdFrameHandler) },
            { "Autostart", true },
            { "Style", "width: 100%; height: 100%; object-fit: cover;" }
        };

        [Inject] public HttpClient? Http { get; set; }
        [Inject] public IHttpClientFactory? HttpClientFactory { get; set; }

        private HttpClient GetHttpClient() => Http ?? HttpClientFactory?.CreateClient() ?? new HttpClient();

        private async Task OpenFaceIdModal()
        {
            Logger.LogInformation("[Profile.FaceID] Opening Face ID biometrics configuration modal in Profile.");
            faceIdStatusMessage = "Инициализация сессии калибровки...";
            faceIdStatusColor = "var(--md-sys-color-primary)";
            currentEnrollmentStage = 1;
            isFaceIdModalOpen = true;
            _enrollmentSessionId = null;
            StateHasChanged();

            try
            {
                var currentUserId = userIdStr;
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    currentUserId = await Settings.GetUserIdAsync();
                }
                if (string.IsNullOrWhiteSpace(currentUserId) && auth != null)
                {
                    var authId = await auth.GetUserIdAsync();
                    if (authId != Guid.Empty) currentUserId = authId.ToString();
                }
                if (string.IsNullOrWhiteSpace(currentUserId) && FormModel?.Id != null && FormModel.Id.Value != Guid.Empty)
                {
                    currentUserId = FormModel.Id.ToString();
                }

                var client = GetHttpClient();
                if (!string.IsNullOrWhiteSpace(currentUserId) && client != null)
                {
                    var req = new Domain.DTOs.FaceEnrollStartRequest { UserId = currentUserId };
                    var url = $"{nav.BaseUri.TrimEnd('/')}/api/faceid/enroll/start";
                    var res = await client.PostAsJsonAsync(url, req, Shared.Serialization.AotJsonContext.Default.FaceEnrollStartRequest);
                    if (res.IsSuccessStatusCode)
                    {
                        var startRes = await res.Content.ReadFromJsonAsync(Shared.Serialization.AotJsonContext.Default.FaceEnrollStartResponse);
                        if (startRes != null)
                        {
                            _enrollmentSessionId = startRes.SessionId;
                            faceIdStatusMessage = startRes.Instruction;
                            currentEnrollmentStage = 1;
                            StateHasChanged();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Profile.FaceID] Failed to start enrollment session");
            }
        }

        private async Task CloseFaceIdModal()
        {
            if (!isFaceIdModalOpen) return;
            isFaceIdModalOpen = false;
            _enrollmentSessionId = null;
            currentEnrollmentStage = 1;
            Logger.LogInformation("[Profile.FaceID] Closing Face ID configuration modal.");
            try
            {
                // Component will dispose automatically on unrendering DynamicComponent
            }
            catch (Exception ex)
            {
                Logger.LogDebug("[Profile.FaceID] Camera streamer error on close: {Message}", ex.Message);
            }
            finally
            {
                await CheckFaceIdStatusAsync();
                StateHasChanged();
            }
        }

        private async Task OnFaceIdFrameHandler(string imgUrl)
        {
            if (string.IsNullOrEmpty(imgUrl) || _isFaceProcessingFrame || string.IsNullOrEmpty(_enrollmentSessionId)) return;
            _isFaceProcessingFrame = true;

            try
            {
                var data = imgUrl.Split(',');
                if (data.Length < 2) return;

                byte[] imageBytes = Convert.FromBase64String(data[1]);
                var client = GetHttpClient();

                if (client != null)
                {
                    var req = new Domain.DTOs.FaceEnrollFrameRequest
                    {
                        SessionId = _enrollmentSessionId,
                        ImageBytes = imageBytes
                    };

                    var url = $"{nav.BaseUri.TrimEnd('/')}/api/faceid/enroll/frame";
                    var res = await client.PostAsJsonAsync(url, req, Shared.Serialization.AotJsonContext.Default.FaceEnrollFrameRequest);
                    if (res.IsSuccessStatusCode)
                    {
                        var frameRes = await res.Content.ReadFromJsonAsync(Shared.Serialization.AotJsonContext.Default.FaceEnrollFrameResponse);
                        if (frameRes != null)
                        {
                            faceIdStatusMessage = frameRes.Message;

                            if (frameRes.Completed)
                            {
                                currentEnrollmentStage = 3;
                                isFaceIdConfigured = true;
                                faceIdStatusColor = "var(--md-sys-color-primary)";
                                StateHasChanged();

                                await Task.Delay(1500);
                                await CloseFaceIdModal();
                                return;
                            }
                            else if (frameRes.Accepted)
                            {
                                currentEnrollmentStage = Math.Min(3, frameRes.CurrentStage + 1);
                                faceIdStatusColor = "var(--md-sys-color-secondary)";
                            }
                            else
                            {
                                faceIdStatusColor = "var(--md-sys-color-tertiary)";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Profile.FaceID] Exception during Face ID enrollment frame processing");
                faceIdStatusMessage = $"Ошибка: {ex.Message}";
                faceIdStatusColor = "var(--md-sys-color-error)";
            }
            finally
            {
                _isFaceProcessingFrame = false;
                StateHasChanged();
            }
        }

        private async Task NavigateToFaceIdSetup()
        {
            await OpenFaceIdModal();
        }

        private async Task RemoveFaceId()
        {
            var currentUserId = userIdStr ?? await Settings.GetUserIdAsync();
            if (string.IsNullOrWhiteSpace(currentUserId) && FormModel?.Id != null && FormModel.Id.Value != Guid.Empty)
            {
                currentUserId = FormModel.Id.ToString();
            }

            if (!string.IsNullOrWhiteSpace(currentUserId) && FaceIdService != null)
            {
                await FaceIdService.DeleteFaceAsync(currentUserId);
                isFaceIdConfigured = false;
                Logger.LogInformation("[Profile.FaceID] Biometrics removed for user {UserId}", currentUserId);
                StateHasChanged();
            }
        }

        private async Task LinkGoogleAccount()
        {
            IsOpen = false;
            var clientId = configuration[SharedKernel.Options.GoogleOptions.GOOGLE_CLIENT_ID] ?? configuration["GOOGLE_CLIENT_ID"];
            Logger.LogInformation("[Profile.Google] Attempting Google Account linking. ClientId configured: {HasClientId}", !string.IsNullOrWhiteSpace(clientId));

            if (string.IsNullOrWhiteSpace(clientId))
            {
                Logger.LogWarning("[Profile.Google] Failed to link Google account: Client ID is not configured (checked keys: {Key1}, {Key2}).",
                    SharedKernel.Options.GoogleOptions.GOOGLE_CLIENT_ID, "GOOGLE_CLIENT_ID");
                return;
            }

            var redirectUri = nav.BaseUri.TrimEnd('/') + "/signin-google-callback";
            var state = "link";
            Logger.LogInformation("[Profile.Google] Opening Google Account Selector popup for linking. RedirectUri: {RedirectUri}", redirectUri);
            var opened = await js.InvokeAsync<bool>("oauthPopupHelper.openGooglePopup", clientId, redirectUri, state);
            if (!opened)
            {
                var scope = "openid email profile";
                var url = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={Uri.EscapeDataString(scope)}&state={state}&prompt=select_account";
                nav.NavigateTo(url, forceLoad: true);
            }
        }

        [Inject] public IPasskeyService? PasskeyService { get; set; }
        private bool isPasskeyConfigured = false;

        private async Task RegisterPasskey()
        {
            if (PasskeyService == null) return;
            var currentUserId = userIdStr ?? await Settings.GetUserIdAsync();
            if (string.IsNullOrWhiteSpace(currentUserId) && FormModel?.Id != null && FormModel.Id.Value != Guid.Empty)
            {
                currentUserId = FormModel.Id.ToString();
            }

            var userName = FormModel?.Name?.FirstName ?? "User";
            var challenge = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var credJson = await PasskeyService.RegisterAsync(currentUserId, userName, challenge);
            if (!string.IsNullOrEmpty(credJson))
            {
                var regDto = System.Text.Json.JsonSerializer.Deserialize<Domain.DTOs.PasskeyRegisterDto>(
                    credJson,
                    Shared.Serialization.AotJsonContext.Default.PasskeyRegisterDto);

                if (regDto != null)
                {
                    regDto.UserId = currentUserId;
                    var token = await Settings.GetAccessTokenAsync();
                    var headers = new Grpc.Core.Metadata();
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        headers.Add("Authorization", $"Bearer {token}");
                    }

                    await authClient.RegisterPasskeyAsync(new Identifying.Application.Protos.PasskeyRegisterCredentials
                    {
                        UserId = currentUserId ?? string.Empty,
                        Id = regDto.Id,
                        RawId = regDto.RawId,
                        ClientDataJson = regDto.ClientDataJSON,
                        AttestationObject = regDto.AttestationObject
                    }, headers);

                    isPasskeyConfigured = true;
                    Logger.LogInformation("[Profile.Passkey] Passkey successfully registered on server for User {UserId}", currentUserId);
                    StateHasChanged();
                }
            }
        }
    }
}


