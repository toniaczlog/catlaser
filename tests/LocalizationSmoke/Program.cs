using System;
using System.Globalization;
using CatLaser;

// Executable smoke test without NuGet test framework dependencies.
// Runs in isolated Windows CI and writes only the ephemeral runner's local profile.
CultureInfo.CurrentUICulture = new CultureInfo("pl-PL");

Assert(UiText.Current == "pl", "pl-PL should default to Polish");
Assert(UiText.T("turn_on") == "WŁĄCZ LASER", "Polish UI text");
Assert(UiText.T("side_cat") == "Zabawa z kotem", "Polish navigation");
Assert(UiText.F("status_session", 42).Contains("42"), "Polish parameter interpolation");

bool notified = false;
UiText.LanguageChanged += () => notified = true;
UiText.SetLanguage("en");
Assert(notified, "Language change event");
Assert(UiText.Current == "en", "English override");
Assert(UiText.T("turn_on") == "TURN LASER ON", "English UI text");
Assert(UiText.T("side_cat") == "Cat games", "English navigation");

notified = false;
UiText.SetLanguage("en");
Assert(!notified, "No redundant event");

UiText.SetLanguage("pl");
Assert(UiText.Current == "pl", "Switch back to Polish");
Console.WriteLine("Localization smoke tests passed.");

static void Assert(bool condition, string label)
{
    if (!condition) throw new InvalidOperationException("Localization test failed: " + label);
}
