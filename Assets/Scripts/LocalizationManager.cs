using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public enum Language { DE, EN }
    public Language currentLanguage = Language.DE;

    private Dictionary<string, Dictionary<Language, string>> translations = new Dictionary<string, Dictionary<Language, string>>();

    // Delegate for components to register to language change events
    public delegate void OnLanguageChanged();
    public event OnLanguageChanged LanguageChangedEvent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            InitializeTranslations();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
        LanguageChangedEvent?.Invoke();
    }

    public string Get(string key)
    {
        if (key != null && key.StartsWith("m16"))
        {
            key = "m8" + key.Substring(3);
        }

        if (translations.ContainsKey(key) && translations[key].ContainsKey(currentLanguage))
        {
            return translations[key][currentLanguage];
        }
        return key; // Fallback to key itself
    }

    private void Add(string key, string de, string en)
    {
        translations[key] = new Dictionary<Language, string>
        {
            { Language.DE, de },
            { Language.EN, en }
        };
    }

    private void InitializeTranslations()
    {
        // ═══════════════════════════════════════════════════════════════
        // OS / Menu Text
        // ═══════════════════════════════════════════════════════════════
        Add("start_game", "SPIEL STARTEN", "START GAME");
        Add("back_to_game", "ZURÜCK ZUM SPIEL", "BACK TO GAME");
        Add("settings", "System-Einstellungen", "System Settings");
        Add("reset_progress", "SPIEL ZURÜCKSETZEN", "RESET PROGRESS");
        Add("credits", "Mitwirkende & Credits", "Contributors & Credits");
        Add("volume_bgm", "🎵 Musik-Lautstärke", "🎵 Music Volume");
        Add("volume_sfx", "🔊 SFX-Lautstärke", "🔊 SFX Volume");
        Add("time_cycle", "Tageszeit-Modus", "Time of Day Mode");
        Add("time_dynamic", "Dynamisch", "Dynamic");
        Add("time_always_day", "Immer Tag", "Always Day");
        Add("time_always_night", "Immer Nacht", "Always Night");
        
        // New OS Elements: Taskbar, Start Menu & Exam
        Add("cert_locked_message", 
            "<b>ZERTIFIKAT GESPERRT / CERTIFICATE LOCKED</b>\n\nDu musst zuerst die Erste-Hilfe-Prüfung (Prüfung.exe) erfolgreich bestehen (mindestens 8/10 Fragen richtig), um dein Zertifikat freizuschalten!\n\n---\n\nYou must first pass the First Aid Exam (Prüfung.exe) with at least 8/10 correct answers to unlock your certificate!", 
            "<b>ZERTIFIKAT GESPERRT / CERTIFICATE LOCKED</b>\n\nDu musst zuerst die Erste-Hilfe-Prüfung (Prüfung.exe) successfully pass (at least 8/10 correct answers) to unlock your certificate!");
        Add("exam_shortcut", "Prüfung.exe", "Exam.exe");
        Add("start_menu_handbook", "📖 Handbuch.exe", "📖 Handbook.exe");
        Add("start_menu_tracker", "📈 Lernfortschritt.exe", "📈 Progress.exe");
        Add("start_menu_exam", "✍️ Prüfung.exe", "✍️ Exam.exe");
        Add("start_menu_cert", "📜 Zertifikat.exe", "📜 Certificate.exe");
        Add("start_menu_terminal", "💻 cmd.exe", "💻 cmd.exe");
        Add("start_menu_bin", "🗑️ Papierkorb", "🗑️ Recycle Bin");
        Add("start_menu_quit", "🚪 Beenden", "🚪 Quit Game");
        
        // HUD / Stats
        Add("hud_stamina", "⚡ Ausdauer", "⚡ Stamina");
        Add("hud_tiredness", "🌙 Müdigkeit", "🌙 Fatigue");
        Add("hud_coins", "Münzen", "Coins");
        Add("interact_prompt", "[E] Untersuchen", "[E] Inspect");
        Add("interact_bed", "[E] Schlafen", "[E] Sleep");
        Add("interact_shop", "[E] Shop öffnen", "[E] Open Shop");

        // ═══════════════════════════════════════════════════════════════
        // Scenario Main Titles
        // ═══════════════════════════════════════════════════════════════
        Add("m1_title", "Fahrradunfall", "Bike Accident");
        Add("m2_title", "Schnittwunde", "Bleeding Wound");
        Add("m3_title", "Bewusstlose Person", "Unconscious Person");
        Add("m4_title", "Verbrennung", "Burn Injury");
        Add("m5_title", "Verschlucken", "Choking Victim");
        Add("m6_title", "Hitzschlag", "Heatstroke");
        Add("m7_title", "Stromschlag", "Electric Shock");
        Add("m8_title", "Vergiftung", "Poisoning Case");
        Add("m9_title", "Massenunfall (Triage)", "Mass Accident (Triage)");
        Add("m10_title", "Knochenbruch", "Bone Fracture");
        Add("m11_title", "Allergischer Schock", "Allergic Shock");
        Add("m12_title", "Ertrinkungsunfall", "Drowning Rescue");
        Add("m13_title", "Diabetischer Schock", "Diabetic Shock");
        Add("m14_title", "Panikattacke", "Panic Attack");
        Add("m15_title", "Verdacht auf Schlaganfall", "Suspected Stroke");
        Add("m17_title", "Hundevergiftung", "Dog Poisoning");
        Add("m18_title", "Herzinfarkt", "Heart Attack");
        Add("m19_title", "Schlangenbiss", "Snakebite");

        // ═══════════════════════════════════════════════════════════════
        // Stroke FAST Details
        // ═══════════════════════════════════════════════════════════════
        Add("stroke_desc", "Eine ältere Person sitzt desorientiert auf einer Bank. Untersuche sie!", "An elderly person sits disoriented on a bench. Inspect them!");
        Add("stroke_face", "GESICHT prüfen (Smile)", "Check FACE (Smile)");
        Add("stroke_arms", "ARME prüfen (Raise)", "Check ARMS (Raise)");
        Add("stroke_speech", "SPRACHE prüfen (Speech)", "Check SPEECH (Talk)");
        Add("stroke_time", "ZEIT prüfen (Call 112)", "Check TIME (Call 112)");
        Add("stroke_face_hint", "Bitte die Person zu lächeln. Hängt ein Mundwinkel herunter?", "Ask the person to smile. Does one side of the mouth droop?");
        Add("stroke_arms_hint", "Bitte die Person beide Arme gleichzeitig nach vorne zu heben. Sinkt ein Arm ab?", "Ask the person to raise both arms. Does one arm drift downward?");
        Add("stroke_speech_hint", "Bitte die Person einen einfachen Satz nachzusprechen. Ist die Sprache verwaschen?", "Ask the person to repeat a simple sentence. Is their speech slurred?");
        Add("stroke_time_hint", "Besteht mindestens ein Symptom? Wähle die richtige Diagnose und rufe 112!", "Does at least one symptom exist? Select the correct diagnosis and call 112!");
        Add("stroke_diag_positive", "Verdacht auf Schlaganfall! (FAST positiv)", "Suspected Stroke! (FAST positive)");
        Add("stroke_diag_negative", "Kein Schlaganfall, nur Schwindel.", "No stroke, just dizziness.");
        Add("stroke_garbled_bubble", "„Ich... fü... fühle mich s... seltsam...“", "\"I... fe... feel s... strange...\"");
        Add("stroke_garbled_bubble_en", "„I... fe... feel s... strange...“", "\"I... fe... feel s... strange...\"");

        // ═══════════════════════════════════════════════════════════════
        // UI Headers
        // ═══════════════════════════════════════════════════════════════
        Add("btn_close", "Schließen", "Close");
        Add("btn_select", "Auswählen", "Select");
        Add("btn_diagnose", "Diagnose stellen", "Diagnose");
        Add("btn_print", "Zertifikat drucken", "Print Certificate");
        Add("btn_clear", "Löschen", "Clear");

        // Certificate Text
        Add("cert_app_title", "Zertifikat.exe", "Certificate.exe");
        Add("cert_title", "ZERTIFIKAT DER ERSTEN HILFE", "FIRST AID CERTIFICATE");
        Add("cert_body1", "Dieses Dokument bescheinigt stolz, dass", "This document proudly certifies that");
        Add("cert_body2", "erfolgreich alle Erste-Hilfe-Simulationen im Park absolviert und Leben gerettet hat.", "has successfully completed all First Aid Park Simulations and saved lives.");
        Add("cert_body3", "Ernannt zum: Zertifizierten Ersthelfer", "Appointed as: Certified First Responder");
        Add("cert_score", "Gesammelte Münzen: ", "Coins collected: ");
        Add("cert_placeholder_name", "Geben Sie Ihren Namen ein", "Enter your name");
        Add("cert_signature", "Unterschrift: AG Serious Games (TU Darmstadt)", "Signature: AG Serious Games (TU Darmstadt)");

        // Mission reports
        Add("report_success", "MISSION ERFOLGREICH!", "MISSION SUCCESSFUL!");
        Add("report_fail", "MISSION FEHLGESCHLAGEN!", "MISSION FAILED!");
        Add("report_coins_earned", "Münzen verdient: ", "Coins earned: ");
        Add("report_hint", "Tipp für das nächste Mal: ", "Tip for next time: ");

        // Shop items
        Add("shop_title", "Ausrüstungs-Shop & Upgrades.exe", "Equipment Shop & Upgrades.exe");
        Add("shop_item_cpr", "CPR-Rhythmus-Stabilisator (Verlangsamt Takt)", "CPR Rhythm Stabilizer (Slows beat)");
        Add("shop_item_quiz", "Quiz-Joker (Entfernt falsche Antworten)", "Quiz Joker (Removes wrong answers)");
        Add("shop_item_bandage", "Erweiterter Verbandskasten (Mehr Fehlertoleranz)", "Advanced Medkit (More error tolerance)");
        Add("shop_item_skin", "Ersthelfer-Skin: ", "First Responder Skin: ");
        Add("shop_btn_buy", "Kaufen (", "Buy (");

        // Notruf / Emergency Call
        Add("call_dispatched", "Rettungsdienst alarmiert!", "Emergency services dispatched!");
        Add("call_w_questions", "Die 5 Ws: Wer, Wo, Was, Wie viele, Warten!", "The 5 Ws: Who, Where, What, How many, Wait!");

        // ═══════════════════════════════════════════════════════════════
        // NEW: Handbook & Debrief UI Labels
        // ═══════════════════════════════════════════════════════════════
        Add("handbook_title", "📖 Erste-Hilfe-Handbuch", "📖 First Aid Handbook");
        Add("handbook_app", "Handbuch.exe", "Handbook.exe");
        Add("handbook_when", "⚠ Erkennung:", "⚠ Recognition:");
        Add("handbook_steps", "✅ Richtige Schritte:", "✅ Correct Steps:");
        Add("handbook_donts", "❌ Häufige Fehler:", "❌ Common Mistakes:");
        Add("handbook_fact", "💡 Wichtiger Fakt:", "💡 Key Fact:");
        Add("tracker_title", "📈 Lernfortschritt", "📈 Learning Progress");
        Add("tracker_app", "Lernfortschritt.exe", "Progress.exe");
        Add("tracker_mastery", "Gesamtfortschritt:", "Total Progress:");
        Add("tracker_ready", "✅ Bereit für die Zertifizierung!", "✅ Ready for Certification!");
        Add("tracker_keep_going", "Weiter üben für die Zertifizierung!", "Keep practicing for certification!");
        Add("debrief_header", "📚 Was habe ich gelernt?", "📚 What did I learn?");
        Add("debrief_steps_label", "Richtige Erste-Hilfe-Schritte:", "Correct First Aid Steps:");
        Add("debrief_mistake_label", "Vermeide diesen Fehler:", "Avoid this mistake:");
        Add("debrief_fact_label", "Medizinischer Fakt:", "Medical Fact:");
        Add("tip_prefix", "💡 Wusstest du?", "💡 Did you know?");

        // ═══════════════════════════════════════════════════════════════
        // HANDBOOK ENTRIES – Real medical first-aid knowledge (DE/EN)
        // Each scenario: _hb_when, _hb_step1-3, _hb_dont, _hb_fact
        // ═══════════════════════════════════════════════════════════════

        // --- Fahrradunfall (m1) ---
        Add("m1_hb_when", "Radfahrer liegt am Boden, evtl. bewusstlos, Helm beschädigt, Schürfwunden sichtbar.", "Cyclist is on the ground, possibly unconscious, helmet damaged, abrasions visible.");
        Add("m1_hb_step1", "1. Unfallstelle absichern, Warndreieck/Warnweste aufstellen.", "1. Secure the accident scene, set up warning triangle/vest.");
        Add("m1_hb_step2", "2. Bewusstsein prüfen: Ansprechen und an den Schultern rütteln.", "2. Check consciousness: Speak to and gently shake the shoulders.");
        Add("m1_hb_step3", "3. Bei Bewusstlosigkeit: Atemkontrolle → Stabile Seitenlage → 112 rufen.", "3. If unconscious: Check breathing → Recovery position → Call 112.");
        Add("m1_hb_dont", "Helm NICHT abnehmen, außer die Atmung ist blockiert!", "Do NOT remove the helmet unless breathing is blocked!");
        Add("m1_hb_fact", "80% der tödlichen Kopfverletzungen bei Radfahrern könnten durch einen Helm verhindert werden.", "80% of fatal head injuries in cyclists could be prevented by wearing a helmet.");

        // --- Schnittwunde / Blutung (m2) ---
        Add("m2_hb_when", "Offene Wunde mit starker Blutung, ggf. aus Arterie (hellrotes, pulsierendes Blut).", "Open wound with heavy bleeding, possibly arterial (bright red, pulsating blood).");
        Add("m2_hb_step1", "1. Einmalhandschuhe anziehen (Eigenschutz!).", "1. Put on disposable gloves (self-protection!).");
        Add("m2_hb_step2", "2. Sterile Kompresse auf die Wunde drücken und Druckverband anlegen.", "2. Press a sterile compress on the wound and apply a pressure bandage.");
        Add("m2_hb_step3", "3. Betroffene Extremität hochlagern. Bei starker Blutung: 112 rufen.", "3. Elevate the affected limb. If heavy bleeding: Call 112.");
        Add("m2_hb_dont", "Fremdkörper NIEMALS aus der Wunde ziehen – sie könnten die Blutung stoppen!", "NEVER pull foreign objects from the wound – they may be stopping the bleeding!");
        Add("m2_hb_fact", "Ein Erwachsener kann bei Verlust von nur 1 Liter Blut bereits einen Schock erleiden.", "An adult can go into shock from losing just 1 liter of blood.");

        // --- Bewusstlose Person (m3) ---
        Add("m3_hb_when", "Person reagiert nicht auf Ansprechen und Rütteln, atmet aber normal.", "Person does not respond to voice or shaking but is breathing normally.");
        Add("m3_hb_step1", "1. Atemwege freimachen: Kopf überstrecken, Kinn anheben.", "1. Open airways: Tilt head back, lift chin.");
        Add("m3_hb_step2", "2. Atmung prüfen: Sehen, Hören, Fühlen (max. 10 Sekunden).", "2. Check breathing: Look, Listen, Feel (max 10 seconds).");
        Add("m3_hb_step3", "3. Stabile Seitenlage durchführen und 112 rufen.", "3. Place in recovery position and call 112.");
        Add("m3_hb_dont", "Bewusstlose Person NIEMALS auf dem Rücken liegen lassen – Erstickungsgefahr durch Erbrochenes!", "NEVER leave an unconscious person on their back – choking hazard from vomit!");
        Add("m3_hb_fact", "Die stabile Seitenlage verhindert, dass die Zunge in den Rachen fällt und die Atemwege blockiert.", "The recovery position prevents the tongue from falling back and blocking the airway.");

        // --- Verbrennung (m4) ---
        Add("m4_hb_when", "Rote Haut, Blasen, starker Schmerz. Bei Verbrühungen: nasse Kleidung.", "Red skin, blisters, severe pain. For scalds: wet clothing.");
        Add("m4_hb_step1", "1. Betroffene Stelle sofort unter fließendes lauwarmes Wasser halten (15–20 Min.).", "1. Hold affected area under cool running water immediately (15–20 min).");
        Add("m4_hb_step2", "2. Verbrannte Kleidung NICHT entfernen, wenn sie an der Haut klebt.", "2. Do NOT remove burned clothing if it sticks to the skin.");
        Add("m4_hb_step3", "3. Steril abdecken (Metalline-Tuch) und 112 bei Verbrennungen > Handflächengröße.", "3. Cover sterilely (metalline sheet) and call 112 if burn area > palm size.");
        Add("m4_hb_dont", "NIEMALS Eis, Butter, Zahnpasta oder Mehl auf Verbrennungen geben!", "NEVER apply ice, butter, toothpaste, or flour to burns!");
        Add("m4_hb_fact", "Verbrennungen ab Grad 2b (tiefe Dermis) heilen nicht mehr von selbst und brauchen Hauttransplantation.", "Burns from degree 2b (deep dermis) can no longer self-heal and require skin grafts.");

        // --- Verschlucken / Erstickung (m5) ---
        Add("m5_hb_when", "Person hält sich den Hals, kann nicht sprechen/husten, Gesicht wird blau.", "Person clutches throat, cannot speak/cough, face turns blue.");
        Add("m5_hb_step1", "1. Person nach vorne beugen, 5 kräftige Rückenschläge zwischen die Schulterblätter.", "1. Lean person forward, give 5 firm back blows between shoulder blades.");
        Add("m5_hb_step2", "2. Wenn wirkungslos: 5x Heimlich-Manöver (Oberbauchkompressionen).", "2. If ineffective: 5x Heimlich maneuver (abdominal thrusts).");
        Add("m5_hb_step3", "3. Rückenschläge und Heimlich abwechseln bis Fremdkörper sich löst oder 112 eintrifft.", "3. Alternate back blows and Heimlich until object dislodges or 112 arrives.");
        Add("m5_hb_dont", "Bei Säuglingen KEIN Heimlich-Manöver – nur Rückenschläge und Brustkompressionen!", "For infants, NO Heimlich maneuver – only back blows and chest compressions!");
        Add("m5_hb_fact", "Erstickung ist bei Kindern unter 5 Jahren eine der häufigsten Unfallursachen im Haushalt.", "Choking is one of the most common causes of accidents in children under 5 at home.");

        // --- Hitzschlag (m6) ---
        Add("m6_hb_when", "Hohe Körpertemperatur (>40°C), trockene/heiße/rote Haut, Verwirrtheit, Bewusstseinstrübung.", "High body temperature (>40°C), dry/hot/red skin, confusion, impaired consciousness.");
        Add("m6_hb_step1", "1. Person sofort in den Schatten bringen und Kleidung lockern.", "1. Move person to shade immediately and loosen clothing.");
        Add("m6_hb_step2", "2. Kühlung: Nasse Tücher auf Stirn, Nacken, Leistengegend legen.", "2. Cool down: Place wet cloths on forehead, neck, groin area.");
        Add("m6_hb_step3", "3. Bei Bewusstsein: Kleine Schlucke Wasser trinken lassen. 112 rufen!", "3. If conscious: Give small sips of water. Call 112!");
        Add("m6_hb_dont", "Person NICHT in eiskaltes Wasser tauchen – Gefahr eines Kreislaufschocks!", "Do NOT immerse person in ice-cold water – risk of circulatory shock!");
        Add("m6_hb_fact", "Ein Hitzschlag ist ein lebensbedrohlicher Notfall, bei dem das Temperaturregulationssystem des Körpers versagt.", "Heatstroke is a life-threatening emergency where the body's temperature regulation system fails.");

        // --- Stromschlag (m7) ---
        Add("m7_hb_when", "Person nach Kontakt mit Strom: Bewusstlos, Verbrennungen (Ein- und Austrittsstelle), Herzrhythmusstörungen.", "Person after electric contact: unconscious, burns (entry and exit marks), cardiac arrhythmia.");
        Add("m7_hb_step1", "1. EIGENSCHUTZ! Stromquelle abschalten oder Person mit nicht-leitendem Gegenstand von Stromquelle trennen.", "1. SELF-PROTECTION! Switch off power source or separate person with non-conductive object.");
        Add("m7_hb_step2", "2. Bewusstsein und Atmung prüfen. Bei Atemstillstand: Reanimation (30:2).", "2. Check consciousness and breathing. If no breathing: CPR (30:2).");
        Add("m7_hb_step3", "3. Verbrennungen steril abdecken. IMMER 112 rufen – auch bei scheinbar leichten Fällen!", "3. Cover burns sterilely. ALWAYS call 112 – even in seemingly mild cases!");
        Add("m7_hb_dont", "NIEMALS eine Person berühren die noch unter Strom steht!", "NEVER touch a person who is still in contact with electricity!");
        Add("m7_hb_fact", "Strom kann Herzrhythmusstörungen verursachen, die erst Stunden nach dem Unfall auftreten – daher immer ins Krankenhaus!", "Electricity can cause cardiac arrhythmias that appear hours after the accident – always go to the hospital!");

        // --- Vergiftung (m8) ---
        Add("m8_hb_when", "Übelkeit, Erbrechen, Durchfall, Verwirrtheit, Bewusstlosigkeit nach Einnahme/Einatmen giftiger Substanzen.", "Nausea, vomiting, diarrhea, confusion, unconsciousness after ingesting/inhaling toxic substances.");
        Add("m8_hb_step1", "1. Giftnotruf kontaktieren (in DE: 030 19240) und Substanz benennen.", "1. Contact poison control (in DE: 030 19240) and identify the substance.");
        Add("m8_hb_step2", "2. Verpackung/Substanzprobe sichern und dem Rettungsdienst übergeben.", "2. Secure packaging/substance sample and hand over to emergency services.");
        Add("m8_hb_step3", "3. Bei Bewusstlosigkeit: Stabile Seitenlage. Bei Atemstillstand: Reanimation.", "3. If unconscious: Recovery position. If no breathing: CPR.");
        Add("m8_hb_dont", "KEIN Erbrechen auslösen – besonders nicht bei Säuren, Laugen oder Benzin!", "Do NOT induce vomiting – especially not with acids, alkalis, or gasoline!");
        Add("m8_hb_fact", "Aktivkohle kann bei manchen Vergiftungen die Giftwirkung abschwächen, aber NUR auf Anweisung des Giftnotrufs!", "Activated charcoal can reduce toxicity in some poisonings, but ONLY on instruction from poison control!");

        // --- Triage / Massenunfall (m9) ---
        Add("m9_hb_when", "Mehrere Verletzte gleichzeitig: Auto-Unfall, Explosion, Großveranstaltung.", "Multiple casualties at once: Car crash, explosion, large event.");
        Add("m9_hb_step1", "1. Überblick verschaffen: Wie viele Verletzte? Wie schwer? → Notruf 112.", "1. Get an overview: How many injured? How severe? → Call 112.");
        Add("m9_hb_step2", "2. Schwerverletzte zuerst versorgen (lebensbedrohliche Blutungen, Atemstillstand).", "2. Treat the most severely injured first (life-threatening bleeding, respiratory arrest).");
        Add("m9_hb_step3", "3. Leichtverletzte beruhigen und anweisen, sich gegenseitig zu helfen.", "3. Calm the walking wounded and instruct them to help each other.");
        Add("m9_hb_dont", "Nicht alle Verletzten gleichzeitig versorgen – priorisieren nach Schwere!", "Don't treat all casualties at once – prioritize by severity!");
        Add("m9_hb_fact", "Im professionellen Rettungsdienst wird das START-Triage-System genutzt: Rot (sofort), Gelb (dringend), Grün (leicht), Schwarz (verstorben).", "Professional rescue services use the START triage system: Red (immediate), Yellow (urgent), Green (minor), Black (deceased).");

        // --- Knochenbruch (m10) ---
        Add("m10_hb_when", "Schwellung, Fehlstellung, starke Schmerzen bei Bewegung, Krepitation (Knirschen).", "Swelling, deformity, severe pain on movement, crepitus (grinding sensation).");
        Add("m10_hb_step1", "1. Betroffene Stelle NICHT bewegen – Bruch so belassen, wie er ist.", "1. Do NOT move the affected area – leave the fracture as it is.");
        Add("m10_hb_step2", "2. Polstern und schienen: Weiche Materialien um den Bruch legen, um Bewegung zu verhindern.", "2. Pad and splint: Place soft materials around the fracture to prevent movement.");
        Add("m10_hb_step3", "3. Kühlen (nicht direkt auf der Haut) und 112 rufen.", "3. Cool (not directly on skin) and call 112.");
        Add("m10_hb_dont", "NIEMALS versuchen, den Knochen einzurenken oder geradezubiegen!", "NEVER try to reset or straighten the bone!");
        Add("m10_hb_fact", "Bei offenen Brüchen (Knochen durchsticht die Haut) besteht hohe Infektionsgefahr – steril abdecken!", "Open fractures (bone piercing the skin) carry high infection risk – cover sterilely!");

        // --- Allergischer Schock / Anaphylaxie (m11) ---
        Add("m11_hb_when", "Atemnotschwellung (Gesicht, Lippen, Zunge), Hautausschlag, Kreislaufprobleme nach Allergenkontakt.", "Breathing difficulty, swelling (face, lips, tongue), rash, circulatory problems after allergen exposure.");
        Add("m11_hb_step1", "1. Allergieauslöser entfernen (z. B. Bienenstachel). Notruf 112.", "1. Remove allergen trigger (e.g., bee stinger). Call 112.");
        Add("m11_hb_step2", "2. Falls vorhanden: Adrenalin-Autoinjektor (EpiPen) in den Oberschenkel verabreichen.", "2. If available: Administer epinephrine auto-injector (EpiPen) into the thigh.");
        Add("m11_hb_step3", "3. Person hinsetzen (bei Atemnot) oder hinlegen mit erhöhten Beinen (bei Kreislaufproblemen).", "3. Sit person up (if breathing difficulty) or lay down with elevated legs (if circulatory problems).");
        Add("m11_hb_dont", "Person NICHT stehen lassen – bei Kreislaufkollaps kann sie fallen und sich verletzen!", "Do NOT let the person stand – if they collapse, they could fall and injure themselves!");
        Add("m11_hb_fact", "Eine Anaphylaxie kann innerhalb von Minuten tödlich sein. Der Adrenalin-Pen ist das einzige lebensrettende Medikament.", "Anaphylaxis can be fatal within minutes. The epinephrine pen is the only life-saving medication.");

        // --- Ertrinkungsunfall (m12) ---
        Add("m12_hb_when", "Person treibt reglos im Wasser oder wurde gerade herausgezogen, atmet nicht oder kaum.", "Person floating motionless in water or just pulled out, not breathing or barely breathing.");
        Add("m12_hb_step1", "1. EIGENSCHUTZ! Nur ins Wasser gehen, wenn du sicher schwimmen kannst. Besser: Rettungsring werfen.", "1. SELF-PROTECTION! Only enter water if you can swim safely. Better: Throw a life ring.");
        Add("m12_hb_step2", "2. Person aus dem Wasser holen. Sofort Atmung prüfen.", "2. Get person out of the water. Immediately check breathing.");
        Add("m12_hb_step3", "3. Bei Atemstillstand: 5 Initialbeatmungen, dann normale Reanimation (30:2). 112 rufen!", "3. If not breathing: 5 initial rescue breaths, then normal CPR (30:2). Call 112!");
        Add("m12_hb_dont", "KEIN Wasser aus dem Magen drücken – das verschlechtert den Zustand!", "Do NOT press water out of the stomach – this worsens the condition!");
        Add("m12_hb_fact", "Ertrinken ist die zweithäufigste Todesursache bei Kindern in Europa. Kinder ertrinken leise – kein Schreien!", "Drowning is the second most common cause of death in children in Europe. Children drown silently – no screaming!");

        // --- Diabetischer Schock / Unterzuckerung (m13) ---
        Add("m13_hb_when", "Zittern, Schweißausbruch, Verwirrtheit, Aggression, Bewusstseinseintrübung bei bekanntem Diabetes.", "Trembling, sweating, confusion, aggression, impaired consciousness in known diabetic.");
        Add("m13_hb_step1", "1. Bei Bewusstsein: Sofort Traubenzucker, Saft oder Cola (kein Light!) geben.", "1. If conscious: Immediately give glucose tablets, juice, or regular cola (not diet!).");
        Add("m13_hb_step2", "2. Person hinsetzen und beruhigen. Zustand alle 5 Minuten kontrollieren.", "2. Sit person down and reassure. Monitor condition every 5 minutes.");
        Add("m13_hb_step3", "3. Bei Bewusstlosigkeit: Stabile Seitenlage, 112 rufen. NICHTS in den Mund geben!", "3. If unconscious: Recovery position, call 112. Give NOTHING by mouth!");
        Add("m13_hb_dont", "KEIN Insulin spritzen – Unterzuckerung wird durch Insulin SCHLIMMER!", "Do NOT inject insulin – hypoglycemia is made WORSE by insulin!");
        Add("m13_hb_fact", "In Deutschland leben ca. 8 Millionen Menschen mit Diabetes. Unterzuckerung kann innerhalb von Minuten lebensgefährlich werden.", "About 8 million people in Germany live with diabetes. Hypoglycemia can become life-threatening within minutes.");

        // --- Panikattacke (m14) ---
        Add("m14_hb_when", "Herzrasen, Atemnot, Schwindel, Brustenge, Todesangst – ohne erkennbare körperliche Ursache.", "Racing heart, breathing difficulty, dizziness, chest tightness, fear of death – no physical cause.");
        Add("m14_hb_step1", "1. Ruhe bewahren und ruhig mit der Person sprechen. Nicht hektisch handeln.", "1. Stay calm and speak quietly to the person. Don't act hectic.");
        Add("m14_hb_step2", "2. Atemübung anleiten: Langsam einatmen (4 Sek.), halten (4 Sek.), ausatmen (6 Sek.).", "2. Guide breathing exercise: Inhale slowly (4 sec), hold (4 sec), exhale (6 sec).");
        Add("m14_hb_step3", "3. Grounding-Technik: Bitte die Person, 5 Dinge zu benennen, die sie sehen kann.", "3. Grounding technique: Ask the person to name 5 things they can see.");
        Add("m14_hb_dont", "Sag NIEMALS 'Reiß dich zusammen' oder 'Das ist doch nichts' – das macht es schlimmer!", "NEVER say 'Pull yourself together' or 'It's nothing' – this makes it worse!");
        Add("m14_hb_fact", "Eine Panikattacke dauert typischerweise 10–30 Minuten und ist NICHT lebensbedrohlich, fühlt sich aber so an.", "A panic attack typically lasts 10–30 minutes and is NOT life-threatening but feels like it.");

        // --- Schlaganfall / FAST (m15) ---
        Add("m15_hb_when", "Plötzlich hängender Mundwinkel, Arm-Schwäche, verwaschene Sprache.", "Sudden drooping face, arm weakness, slurred speech.");
        Add("m15_hb_step1", "1. FAST-Test durchführen: Face, Arms, Speech, Time.", "1. Perform FAST test: Face, Arms, Speech, Time.");
        Add("m15_hb_step2", "2. Sofort 112 rufen und 'Verdacht auf Schlaganfall' mitteilen.", "2. Immediately call 112 and report 'suspected stroke'.");
        Add("m15_hb_step3", "3. Person mit erhöhtem Oberkörper lagern (30°). Nichts zu essen/trinken geben!", "3. Position person with upper body elevated (30°). Give nothing to eat/drink!");
        Add("m15_hb_dont", "KEINE Medikamente geben – auch kein Aspirin! Das kann die Blutung verschlimmern.", "Give NO medication – not even aspirin! It could worsen the bleeding.");
        Add("m15_hb_fact", "Bei einem Schlaganfall sterben pro Minute ca. 1,9 Millionen Nervenzellen. Jede Minute zählt: 'Time is Brain!'", "During a stroke, about 1.9 million nerve cells die per minute. Every minute counts: 'Time is Brain!'");

        // --- Herzinfarkt (m18) ---
        Add("m18_hb_when", "Starker Druck oder Schmerz in der Brust, ausstrahlend in Arm, Hals oder Bauch. Atemnot, kalter Schweiß.", "Severe pressure or pain in the chest, radiating to arm, neck or stomach. Shortness of breath, cold sweat.");
        Add("m18_hb_step1", "1. Sofort Notruf 112 absetzen ('Verdacht auf Herzinfarkt').", "1. Immediately call 112 ('suspected heart attack').");
        Add("m18_hb_step2", "2. Oberkörper hochlagern, enge Kleidung lockern, Person beruhigen.", "2. Elevate upper body, loosen tight clothing, calm the person.");
        Add("m18_hb_step3", "3. Bei Herzstillstand: Sofort mit Reanimation beginnen und Defibrillator (AED) einsetzen.", "3. If cardiac arrest: Start CPR immediately and use a defibrillator (AED).");
        Add("m18_hb_dont", "Die Person NICHT alleine lassen und NICHT mehr laufen lassen!", "Do NOT leave the person alone and do NOT let them walk anymore!");
        Add("m18_hb_fact", "Ein Defibrillator (AED) erklärt jeden Schritt laut und kann nichts falsch machen – er schockt nur, wenn nötig.", "A defibrillator (AED) explains every step out loud and can't do anything wrong - it only shocks if necessary.");

        // --- Schlangenbiss (m19) ---
        Add("m19_hb_when", "Zwei kleine Stichwunden, Schwellung, starke Schmerzen, Übelkeit nach Schlangenkontakt.", "Two small puncture wounds, swelling, severe pain, nausea after snake contact.");
        Add("m19_hb_step1", "1. Ruhe bewahren! Betroffenes Körperteil ruhigstellen (nicht bewegen).", "1. Stay calm! Immobilize the affected body part (do not move).");
        Add("m19_hb_step2", "2. Notruf 112 absetzen und Schlange beschreiben (Farbe, Muster).", "2. Call 112 and describe the snake (color, pattern).");
        Add("m19_hb_step3", "3. Bei stark giftigen Schlangen: Tourniquet oder Druckverband (nur von Profis).", "3. For highly venomous snakes: Tourniquet or pressure bandage (professionals only).");
        Add("m19_hb_dont", "Das Gift NIEMALS aussaugen, die Wunde NICHT einschneiden und KEIN Eis auflegen!", "NEVER suck out the venom, do NOT cut the wound and apply NO ice!");
        Add("m19_hb_fact", "Bewegung beschleunigt die Ausbreitung des Giftes im Körper, da der Blut- und Lymphfluss steigt.", "Movement accelerates the spread of the venom in the body as blood and lymph flow increases.");

        // ═══════════════════════════════════════════════════════════════
        // MEDICAL TIPS – Rotating "Did you know?" facts
        // ═══════════════════════════════════════════════════════════════
        Add("tip_01", "Bei einem Herzstillstand sinkt die Überlebenschance pro Minute ohne Reanimation um ca. 10%.", "In cardiac arrest, the survival chance drops by about 10% per minute without CPR.");
        Add("tip_02", "Die stabile Seitenlage schützt bewusstlose Patienten vor dem Ersticken an Erbrochenem.", "The recovery position protects unconscious patients from choking on vomit.");
        Add("tip_03", "Verbrennungen NIEMALS mit Eis kühlen – nur lauwarmes, fließendes Wasser verwenden!", "NEVER cool burns with ice – only use lukewarm running water!");
        Add("tip_04", "Der Notruf 112 funktioniert europaweit, auch ohne SIM-Karte und Guthaben.", "The emergency number 112 works across Europe, even without a SIM card or credit.");
        Add("tip_05", "Bei starker Blutung: Druckverband > 5 Minuten halten. Nicht nachschauen, ob es noch blutet!", "For heavy bleeding: Hold pressure bandage > 5 minutes. Don't peek to check if it's still bleeding!");
        Add("tip_06", "Ein AED (Defibrillator) kann von jedem benutzt werden – die Sprachanweisungen leiten dich durch.", "An AED (defibrillator) can be used by anyone – the voice instructions guide you through.");
        Add("tip_07", "Beim Heimlich-Manöver: Faust zwischen Bauchnabel und Brustbein platzieren, ruckartig nach oben ziehen.", "For the Heimlich maneuver: Place fist between navel and breastbone, thrust sharply upward.");
        Add("tip_08", "In Deutschland ist Erste Hilfe Pflicht! Unterlassene Hilfeleistung ist strafbar (§ 323c StGB).", "In Germany, first aid is mandatory! Failure to render assistance is punishable (§ 323c StGB).");
        Add("tip_09", "Die häufigste Todesursache bei unter 45-Jährigen in Deutschland ist der Unfall – Erste Hilfe rettet Leben!", "The most common cause of death in people under 45 in Germany is accidents – first aid saves lives!");
        Add("tip_10", "Reanimation: 30 Brustkompressionen, dann 2 Beatmungen. Drucktiefe: 5–6 cm. Frequenz: 100–120/min.", "CPR: 30 chest compressions, then 2 rescue breaths. Depth: 5–6 cm. Rate: 100–120/min.");
        Add("tip_11", "Bei Verdacht auf Wirbelsäulenverletzung: Person NICHT bewegen, außer bei unmittelbarer Lebensgefahr!", "If spinal injury is suspected: Do NOT move the person unless there is immediate danger to life!");
        Add("tip_12", "Kinder unter 1 Jahr: Reanimation mit 2 Fingern auf dem Brustbein, nicht mit der ganzen Hand.", "Children under 1 year: CPR with 2 fingers on the sternum, not with the full hand.");
        Add("tip_13", "Die Giftzentrale Deutschland ist 24/7 erreichbar: 030 19240.", "The German Poison Control Center is available 24/7: 030 19240.");
        Add("tip_14", "Bei Unterkühlung: Person langsam aufwärmen. Kein heißes Bad! Warme Decken und warme Getränke.", "For hypothermia: Warm person slowly. No hot bath! Warm blankets and warm drinks.");
        Add("tip_15", "Erste-Hilfe-Kurse sollten alle 2 Jahre aufgefrischt werden, um die Kenntnisse aktuell zu halten.", "First aid courses should be refreshed every 2 years to keep knowledge current.");
        Add("tip_16", "Bei einem Asthma-Anfall: Person aufrecht hinsetzen, Lippenbremse (durch gespitzte Lippen ausatmen) anleiten.", "During an asthma attack: Sit person upright, guide pursed-lip breathing (exhale through pursed lips).");
        Add("tip_17", "Zecken mit einer Pinzette hautnah greifen und gerade herausziehen. NICHT drehen!", "Grab ticks with tweezers close to the skin and pull straight out. Do NOT twist!");
        Add("tip_18", "Bei Nasenbluten: Kopf nach VORNE beugen, Nasenflügel 10 Min. zusammendrücken.", "For nosebleeds: Lean head FORWARD, pinch nostrils together for 10 minutes.");
        Add("tip_19", "Ein Tourniquet (Abbindung) ist die letzte Option bei Extremitätenblutungen – nur wenn Druckverband versagt.", "A tourniquet is the last resort for limb bleeding – only when pressure bandage fails.");
        Add("tip_20", "Der FAST-Test für Schlaganfall dauert nur 60 Sekunden – Face, Arms, Speech, Time – und kann Leben retten!", "The FAST test for stroke takes only 60 seconds – Face, Arms, Speech, Time – and can save lives!");
    }
}
