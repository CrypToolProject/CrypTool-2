# CrypTool-Komponenten ohne Hilfe

Zuletzt geprüft: 2026-09-26

Die Bestandsaufnahme erfasst jedes `PluginInfo`-Attribut unterhalb von `CrypPlugins` und prüft, ob die dort konfigurierte XML-Hilfe als Ressource in das jeweilige Komponentenprojekt eingebunden ist.

## Zusammenfassung

- Registrierte Komponenten: 217
- Komponenten mit konfigurierter XML-Hilfe: 214
- Komponenten ohne konfigurierte XML-Hilfe: 3
- Geprüfte Hilfe-XML-Dateien: 207

## Komponenten ohne konfigurierte Hilfe

| Komponente | Quelldatei | Aktueller `PluginInfo`-Wert | Hinweis |
| --- | --- | --- | --- |
| `AudioInput` | `CrypPlugins/AudioInput/AudioInput.cs` | `null` | Im Projekt wurde keine Komponentenhilfe-XML gefunden. |
| `AudioOutput` | `CrypPlugins/AudioOutput/AudioOutput.cs` | `null` | Im Projekt wurde keine Komponentenhilfe-XML gefunden. |
| `FEAL` | `CrypPlugins/FEAL/FEAL.cs` | leere Zeichenfolge | `CrypPlugins/FEAL/userdoc.xml` existiert und ist als Ressource eingebunden, wird aber nicht von `PluginInfo` referenziert. |

Alle 214 konfigurierten Hilfeverweise lassen sich zu vorhandenen XML-Ressourcen auflösen. Die Liste basiert auf den registrierten Komponenten und nicht auf Verzeichnisnamen; verschachtelte Projektordner und gemeinsam verwendete Hilfedokumente werden daher korrekt berücksichtigt.
