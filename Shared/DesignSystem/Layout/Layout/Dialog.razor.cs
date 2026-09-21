
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;



namespace Shared.Layout
{
    public partial class Dialog
    {
        public static Dialog? Instance { get; set; } = default!;
        public static RenderFragment? Window
        {
            get => Instance?.ChildContent;
            set
            {
                if (Instance is not null)
                {
                    Instance.ChildContent = value;
                    Instance.StateHasChanged();
                }
            }
        }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public RenderFragment? Headline { get; set; }
        [Parameter] public RenderFragment? Actions { get; set; }
        [Parameter] public bool Open { get; set; }
        [Parameter] public EventCallback<EventArgs> OnClose { get; set; }

        protected override Task OnInitializedAsync()
        {
            Instance = this;
            return base.OnInitializedAsync();
        }

        public static Task Close()
        {
            if (Instance is not null)
            {
                Instance.ChildContent = null;
                Instance.StateHasChanged();
            }
            return Task.CompletedTask;
        }

        public static async Task Create<T>(MouseEventArgs e)
        {
            Instance?.Open = true;
            Window = b => b.AddContent(0, builder =>
            {
                builder.OpenComponent(1, typeof(T));
                builder.AddAttribute(2, nameof(Open), Instance?.Open);
                builder.CloseComponent();
            });
            await Task.CompletedTask;
        }
        public static async Task Details<T, A>(MouseEventArgs e, A model)
        {
            Instance?.Open = true;
            Window = b => b.AddContent(0, builder =>
            {
                builder.OpenComponent(1, typeof(T));
                builder.AddAttribute(2, typeof(A).Name, model);
                builder.AddAttribute(3, nameof(Open), Instance?.Open);
                builder.CloseComponent();
            });
            await Task.CompletedTask;
        }
        public static async Task Edit<T, A>(MouseEventArgs e, A model)
        {
            Instance?.Open = true;
            Window = b => b.AddContent(0, builder =>
            {
                builder.OpenComponent(1, typeof(T));
                Console.WriteLine(typeof(A).Name);
                builder.AddAttribute(2, typeof(A).Name, model);
                builder.AddAttribute(3, nameof(Open), Instance?.Open);
                builder.CloseComponent();
            });
            await Task.CompletedTask;
        }
        public static async Task Delete<T, A>(MouseEventArgs e, A model)
        {
            Instance?.Open = true;
            Window = b => b.AddContent(0, builder =>
            {
                builder.OpenComponent(1, typeof(T));
                builder.AddAttribute(2, typeof(A).Name, model);
                builder.AddAttribute(3, nameof(Open), Instance?.Open);
                builder.CloseComponent();
            });
            await Task.CompletedTask;
        }
    }
}
