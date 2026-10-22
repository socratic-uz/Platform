using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Material.Web.Components;

public partial class Select : InputBase<string>
{
    public enum SelectVariant
    {
        Filled,
        Outlined
    }

    public class SelectOption
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string? SupportingText { get; set; }
        public string? TrailingText { get; set; }
        public string? LeadingIcon { get; set; }
        public string? TrailingIcon { get; set; }
        public bool Disabled { get; set; } = false;

        public SelectOption() { }

        public SelectOption(string value, string text)
        {
            Value = value;
            Text = text;
        }

        public SelectOption(string value, string text, string? supportingText = null, string? leadingIcon = null, string? trailingIcon = null)
        {
            Value = value;
            Text = text;
            SupportingText = supportingText;
            LeadingIcon = leadingIcon;
            TrailingIcon = trailingIcon;
        }
    }

    protected ElementReference selectElement;

    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Parameter] public SelectVariant Variant { get; set; } = SelectVariant.Filled;
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string SupportingText { get; set; } = "";
    [Parameter] public string ErrorText { get; set; } = "";
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool Required { get; set; } = false;
    [Parameter] public bool Quick { get; set; } = false;
    [Parameter] public bool NoAsterisk { get; set; } = false;
    [Parameter] public string MenuPositioning { get; set; } = "popover";
    [Parameter] public bool ClampMenuWidth { get; set; } = false;
    [Parameter] public int TypeaheadDelay { get; set; } = 200;
    [Parameter] public bool HasLeadingIcon { get; set; } = false;
    [Parameter] public string LeadingIcon { get; set; } = "";
    [Parameter] public string MenuAlign { get; set; } = "start";

    [Parameter] public Expression<Func<string>>? For { get; set; }
    [Parameter] public bool ShowValidationMessage { get; set; } = true;

    [Parameter] public List<SelectOption>? Options { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public EventCallback<EventArgs> OnOpening { get; set; }
    [Parameter] public EventCallback<EventArgs> OnOpened { get; set; }
    [Parameter] public EventCallback<EventArgs> OnClosing { get; set; }
    [Parameter] public EventCallback<EventArgs> OnClosed { get; set; }

    protected bool HasValidationErrors => EditContext?.GetValidationMessages(FieldIdentifier).Any() == true;

    protected string GetSupportingText()
    {
        if (HasValidationErrors && ShowValidationMessage)
            return "";
        return SupportingText;
    }

    protected string GetErrorText()
    {
        if (HasValidationErrors && ShowValidationMessage)
        {
            var messages = EditContext?.GetValidationMessages(FieldIdentifier);
            return messages?.FirstOrDefault() ?? ErrorText;
        }
        return ErrorText;
    }

    protected Task HandleInput(ChangeEventArgs e)
    {
        CurrentValueAsString = e.Value?.ToString();
        return Task.CompletedTask;
    }

    protected Task HandleChange(ChangeEventArgs e)
    {
        CurrentValueAsString = e.Value?.ToString();
        return Task.CompletedTask;
    }

    protected override void OnParametersSet()
    {
        if (For != null && EditContext != null)
        {
            FieldIdentifier = FieldIdentifier.Create(For);
        }
        base.OnParametersSet();
    }

    protected override bool TryParseValueFromString(string? value, out string result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value ?? "";
        validationErrorMessage = null;
        return true;
    }
}
