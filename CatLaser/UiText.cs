using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CatLaser;

/// <summary>App chrome localization. Defaults to the Windows UI language, unless explicitly overridden.</summary>
internal static class UiText
{
    private static readonly string LanguageFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CatLaser", "language.txt");

    private static readonly Dictionary<string, (string En, string Pl)> Strings = new(StringComparer.Ordinal)
    {
        ["adaptive_power"] = ("Save energy when manual laser is still", "Oszczędzaj energię przy nieruchomym laserze"),
        ["window_title"] = ("CatLaser 0.3 · Laser Playground", "CatLaser 0.3 · Laser dla kota"),
        ["tagline"] = ("Make your cursor a little more magical.", "Zmień kursor w efektowny laser."),
        ["language"] = ("Language", "Język"),
        ["turn_on"] = ("TURN LASER ON", "WŁĄCZ LASER"),
        ["turn_off"] = ("TURN LASER OFF", "WYŁĄCZ LASER"),
        ["status_off"] = ("Laser off · Windows cursors restored", "Laser wyłączony · przywrócono kursory Windows"),
        ["status_manual"] = ("Laser active · following the mouse", "Laser aktywny · podąża za myszą"),
        ["status_no_cursor"] = ("Laser active · cursor replacement unavailable", "Laser aktywny · nie można zmienić kursora"),
        ["status_rest"] = ("Cat mode · rest break", "Zabawa z kotem · przerwa"),
        ["status_session"] = ("Cat mode · {0} s remaining", "Zabawa z kotem · zostało {0} s"),
        ["fps_measure"] = ("{0} FPS measured · limit {1} FPS", "{0} FPS zmierzone · limit {1} FPS"),
        ["side_appearance"] = ("Laser", "Laser"),
        ["side_cat"] = ("Cat games", "Zabawa z kotem"),
        ["side_session"] = ("Sessions", "Sesje"),
        ["side_performance"] = ("Performance", "Wydajność"),
        ["side_presets"] = ("My presets", "Moje presety"),
        ["side_help"] = ("Shortcuts & help", "Skróty i pomoc"),
        ["head_appearance"] = ("Light studio", "Studio światła"),
        ["desc_appearance"] = ("Choose a look and tune the brightness, glow and motion trail.", "Wybierz efekt i dopasuj jasność, poświatę oraz świetlny ślad."),
        ["label_style"] = ("Laser effect", "Efekt lasera"),
        ["label_color"] = ("Light color", "Kolor światła"),
        ["label_glow"] = ("Glow intensity", "Intensywność poświaty"),
        ["label_size"] = ("Laser size", "Wielkość lasera"),
        ["label_trail"] = ("Trail length", "Długość śladu"),
        ["label_jitter"] = ("Organic jitter", "Naturalne drżenie"),
        ["toggle_trail"] = ("Light trail", "Świetlny ślad"),
        ["toggle_wave"] = ("Shockwave on click or sudden stop", "Fala świetlna przy kliknięciu lub nagłym zatrzymaniu"),
        ["head_cat"] = ("Cat playground", "Plac zabaw dla kota"),
        ["desc_cat"] = ("Pick how the laser moves without moving your real mouse cursor.", "Wybierz zachowanie lasera bez poruszania prawdziwym kursorem myszy."),
        ["label_mode"] = ("Play mode", "Tryb zabawy"),
        ["label_speed"] = ("Movement speed", "Prędkość ruchu"),
        ["hint_cat"] = ("Hide & Seek draws virtual hiding spots. Catch Me responds to clicks near the light. Automated movement never controls your mouse.", "Chowanego wyświetla wirtualne kryjówki. Złap mnie reaguje na kliknięcia w pobliżu światła. Tryby automatyczne nie poruszają myszą."),
        ["head_session"] = ("Session studio", "Studio sesji"),
        ["desc_session"] = ("Set the length of the game and breaks for automated modes.", "Ustaw czas zabawy i przerwy w trybach automatycznych."),
        ["label_session"] = ("Play duration", "Czas zabawy"),
        ["label_rest_every"] = ("Pause every", "Przerwa co"),
        ["label_rest_duration"] = ("Pause duration", "Długość przerwy"),
        ["session_hint"] = ("At the end of the session CatLaser turns the effect off and restores your Windows cursors. Changing a play mode restarts the timer.", "Po zakończeniu sesji CatLaser wyłącza efekt i przywraca kursory Windows. Zmiana trybu zabawy uruchamia licznik od nowa."),
        ["head_performance"] = ("Performance center", "Centrum wydajności"),
        ["desc_performance"] = ("Adjust how much work the animation does each second.", "Dostosuj obciążenie komputera i płynność animacji."),
        ["label_performance"] = ("Performance profile", "Profil wydajności"),
        ["label_fps"] = ("Frame-rate limit", "Limit klatek na sekundę"),
        ["perf_hint"] = ("Eco reduces particles and targets 30 FPS. Balanced targets 60 FPS. Ultra targets 120 FPS. Actual performance depends on your monitor and Windows.", "Eco ogranicza efekty i celuje w 30 FPS. Balanced to 60 FPS, a Ultra to 120 FPS. Rzeczywista płynność zależy od monitora i Windows."),
        ["head_presets"] = ("Your own laser presets", "Twoje ustawienia lasera"),
        ["desc_presets"] = ("Save up to 25 configurations locally. No account or cloud sync.", "Zapisz do 25 zestawów ustawień lokalnie, bez konta i chmury."),
        ["label_preset_name"] = ("Preset name", "Nazwa presetu"),
        ["label_saved"] = ("Saved presets", "Zapisane presety"),
        ["save_preset"] = ("Save / update", "Zapisz / aktualizuj"),
        ["load_preset"] = ("Load preset", "Wczytaj preset"),
        ["delete_preset"] = ("Delete", "Usuń"),
        ["preset_default"] = ("My laser", "Mój laser"),
        ["preset_saved"] = ("Preset saved on this computer.", "Preset zapisany na tym komputerze."),
        ["preset_invalid"] = ("Enter a name up to 40 characters (maximum 25 presets).", "Podaj nazwę do 40 znaków (maksymalnie 25 presetów)."),
        ["preset_select"] = ("First select a saved preset.", "Najpierw wybierz zapisany preset."),
        ["preset_loaded"] = ("Loaded: {0}", "Wczytano: {0}"),
        ["preset_deleted"] = ("Deleted: {0}", "Usunięto: {0}"),
        ["empty_presets"] = ("No presets saved yet.", "Nie masz jeszcze zapisanych presetów."),
        ["head_help"] = ("Shortcuts & safety", "Skróty i bezpieczeństwo"),
        ["desc_help"] = ("Control CatLaser without opening its window.", "Steruj CatLaser bez otwierania okna."),
        ["hotkey_toggle"] = ("Turn laser on / off", "Włącz / wyłącz laser"),
        ["hotkey_effect"] = ("Next laser effect", "Następny efekt lasera"),
        ["hotkey_color"] = ("Next color", "Następny kolor"),
        ["hotkey_mode"] = ("Next cat game", "Następny tryb zabawy"),
        ["hotkey_speed"] = ("Increase / decrease speed", "Zwiększ / zmniejsz prędkość"),
        ["safety_title"] = ("Important", "Ważne"),
        ["safety_info"] = ("Closing this window keeps CatLaser running in the system tray. Use Exit to close it completely and restore Windows cursors. Supervise your cat near the screen and finish with a real toy.", "Zamknięcie tego okna pozostawia CatLaser w zasobniku systemowym. Użyj przycisku Zakończ, aby zamknąć program i przywrócić kursory Windows. Pilnuj kota przy monitorze, a zabawę zakończ prawdziwą zabawką."),
        ["exit"] = ("Exit CatLaser", "Zakończ CatLaser"),
        ["footer"] = ("Works locally · no account · no telemetry", "Działa lokalnie · bez konta · bez telemetrii"),
        ["select_placeholder"] = ("Choose...", "Wybierz..."),
        ["style_real"] = ("Real", "Realistyczny"),
        ["style_neon"] = ("Neon", "Neon"),
        ["style_cyber"] = ("Cyber", "Cyber"),
        ["style_photoreal"] = ("Photoreal", "Fotorealistyczny"),
        ["style_comet"] = ("Comet", "Kometa"),
        ["style_electric"] = ("Electric", "Elektryczny"),
        ["style_prism"] = ("Prism", "Pryzmat"),
        ["style_plasma"] = ("Plasma", "Plazma"),
        ["mode_manual"] = ("Manual", "Ręczny"),
        ["mode_ricochet"] = ("Ricochet", "Rykoszet"),
        ["mode_smartprey"] = ("Smart Prey", "Sprytna zdobycz"),
        ["mode_mousehunt"] = ("Mouse Hunt", "Polowanie na mysz"),
        ["mode_hideseek"] = ("Hide & Seek", "Chowanego"),
        ["mode_catchme"] = ("Catch Me!", "Złap mnie!"),
        ["color_red"] = ("Red", "Czerwony"),
        ["color_green"] = ("Green", "Zielony"),
        ["color_blue"] = ("Blue", "Niebieski"),
        ["color_violet"] = ("Violet", "Fioletowy"),
        ["color_amber"] = ("Amber", "Bursztynowy"),
        ["color_cyan"] = ("Cyan", "Cyjan"),
        ["profile_eco"] = ("Eco", "Oszczędny"),
        ["profile_balanced"] = ("Balanced", "Zrównoważony"),
        ["profile_ultra"] = ("Ultra", "Ultra"),
        ["auto"] = ("Automatic", "Automatycznie"),
        ["none"] = ("None", "Brak"),
        ["seconds"] = ("{0} seconds", "{0} sekund"),
        ["minutes"] = ("{0} minutes", "{0} minuty"),
        ["minutes_five"] = ("5 minutes", "5 minut"),
        ["tray_open"] = ("Open CatLaser", "Otwórz CatLaser"),
        ["tray_toggle"] = ("Toggle laser (Ctrl+Alt+L)", "Włącz / wyłącz laser (Ctrl+Alt+L)"),
        ["tray_cat"] = ("Cat mode · Smart Prey", "Tryb kota · Sprytna zdobycz"),
        ["tray_restore"] = ("Restore Windows cursors", "Przywróć kursory Windows"),
        ["tray_exit"] = ("Exit", "Zakończ"),
        ["style_halopulse"] = ("Soft halo pulse", "Pulsujące halo"),
        ["style_twinspot"] = ("Twin spot", "Podwójny punkt"),
        ["style_lighttunnel"] = ("Light tunnel", "Świetlny tunel"),
        ["style_softstar"] = ("Soft star", "Świetlna gwiazda"),
        ["mode_spiral"] = ("Spiral", "Spirala"),
        ["mode_figureeight"] = ("Figure eight", "Ósemka"),
        ["mode_orbit"] = ("Orbit", "Orbita"),
        ["label_monitor"] = ("Play monitor", "Monitor zabawy"),
        ["monitor_follow"] = ("Follow mouse monitor", "Monitor aktywnej myszy"),
        ["label_area"] = ("Play region (% of monitor)", "Obszar zabawy (% monitora)"),
        ["area_x"] = ("Left", "Od lewej"),
        ["area_y"] = ("Top", "Od góry"),
        ["area_width"] = ("Width", "Szerokość"),
        ["area_height"] = ("Height", "Wysokość"),
        ["label_orbit"] = ("Orbit radius", "Promień orbity"),
        ["pause_fullscreen"] = ("Pause in fullscreen apps", "Wstrzymaj w programach pełnoekranowych"),
        ["playlist_title"] = ("Laser playlists", "Playlisty lasera"),
        ["playlist_desc"] = ("Build a custom play sequence. A manual step means a dark rest break.", "Zbuduj sekwencję zabawy. Krok ręczny oznacza przerwę bez lasera."),
        ["playlist_step"] = ("Add a step", "Dodaj krok"),
        ["playlist_rest"] = ("Rest (laser off)", "Przerwa (bez lasera)"),
        ["playlist_empty"] = ("No steps yet", "Brak kroków"),
        ["playlist_add"] = ("Add step", "Dodaj krok"),
        ["playlist_clear"] = ("Clear sequence", "Wyczyść sekwencję"),
        ["playlist_default"] = ("My playlist", "Moja playlista"),
        ["playlist_save"] = ("Save playlist", "Zapisz playlistę"),
        ["playlist_saved"] = ("Playlist saved. Reopen this tab to refresh the saved list.", "Zapisano playlistę. Otwórz ponownie zakładkę, aby odświeżyć listę."),
        ["playlist_error"] = ("Select 1–20 steps and use a valid name.", "Wybierz 1–20 kroków i poprawną nazwę."),
        ["playlist_start"] = ("Start selected playlist", "Uruchom wybraną playlistę"),
        ["stop_playlist"] = ("Stop playlist", "Zatrzymaj playlistę"),
        ["hotkey_edit"] = ("Global keyboard shortcuts", "Globalne skróty klawiszowe"),
        ["hotkey_faster"] = ("Move faster", "Przyspiesz ruch"),
        ["hotkey_slower"] = ("Move slower", "Zwolnij ruch"),
        ["hotkey_apply"] = ("Apply shortcuts", "Zastosuj skróty"),
        ["hotkey_duplicate"] = ("Shortcuts must be unique.", "Skróty nie mogą się powtarzać."),
        ["hotkey_conflict"] = ("At least one shortcut is already used by Windows or another app. Previous bindings restored.", "Co najmniej jeden skrót jest zajęty. Przywrócono poprzednie ustawienia."),
        ["hotkey_saved"] = ("Shortcuts saved.", "Zapisano skróty."),
        ["hotkey_not_saved"] = ("Shortcuts active, but the settings file could not be saved.", "Skróty działają, ale nie udało się zapisać ustawień."),
        ["tray_modes"] = ("Play modes", "Tryby zabawy"),
        ["tray_colors"] = ("Laser color", "Kolor lasera"),
        ["tray_presets"] = ("Presets", "Presety"),
        ["tray_playlists"] = ("Playlists", "Playlisty"),
        ["tray_faster"] = ("Faster +10%", "Szybciej +10%"),
        ["tray_slower"] = ("Slower -10%", "Wolniej -10%"),
        ["tray_tooltip"] = ("CatLaser · Windows laser pointer", "CatLaser · laserowy kursor Windows")
    };

    private static string _language = ReadLanguage();

    public static string Current => _language;
    public static event Action? LanguageChanged;

    public static string T(string key)
    {
        if (!Strings.TryGetValue(key, out var pair)) return key;
        return _language == "pl" ? pair.Pl : pair.En;
    }

    public static string F(string key, params object[] values) =>
        string.Format(CultureInfo.CurrentCulture, T(key), values);

    public static void SetLanguage(string value)
    {
        if (value != "pl" && value != "en") return;
        if (_language == value) return;
        _language = value;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LanguageFile)!);
            var temp = LanguageFile + ".tmp";
            File.WriteAllText(temp, value);
            File.Move(temp, LanguageFile, true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        LanguageChanged?.Invoke();
    }

    private static string ReadLanguage()
    {
        try
        {
            if (File.Exists(LanguageFile))
            {
                string value = File.ReadAllText(LanguageFile).Trim().ToLowerInvariant();
                if (value is "pl" or "en") return value;
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }

        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals(
            "pl", StringComparison.OrdinalIgnoreCase) ? "pl" : "en";
    }
}
