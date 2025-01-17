# Lern-Periode-8
## 10.01.2025 bis 31.01.2025

# Grob-Planung
Was die Noten angeht, so geht es mir ziemlich schlecht, und wahrscheinlich werde ich provisorisch werden, aber natürlich werde ich mich bemühen, so gut es geht. Es ist nur so, dass wir in allen Fächern jetzt ziemlich schwierige Themen für mich persönlich durchgehen, und meine persönliche Verfassung lässt viel zu wünschen übrig. Aber trotzdem versuche ich es weiter und gebe die Hoffnung nicht auf, also wird alles gut werden.

Was die Module angeht, ist es jetzt auch ziemlich schlimm für mich, es sind nicht die Module, die ich persönlich gerne verstehen würde, aber ich muss es und ich versuche, sie trotz meiner persönlichen Unzufriedenheit zu verstehen.

Für dieses LP Projekt habe ich bereits ein Thema ausgewählt - es wird ein Toolkit für die einfache Erstellung von Levels in Form einer LevelData.asset Datei sein, mit dem ich in Zukunft viel schneller und einfacher Levels für mein flewe Spiel aus LP 7 erstellen kann.

Für LP 9 habe ich auch ein Thema fertig, und ich bin sehr froh darüber (dass ich mir nicht erst ein Thema ausdenken muss, sondern schon weiss und Plus oder Minus einen Plan im Kopf habe).


## 10.01
- [x] Erstellen eines Editorfensters mit einer Grundstruktur und einem Menü für den Zugriff über „Flow Free/Level Editor“.
- [x] Implementieren ein Raster-Rendering-System mit anpassbaren Abmessungen (width und height) und Rasterlinienanzeige.
- [x] Hinzufügen einer Basispalette mit visueller Anzeige der ausgewählten Farben und Blockierung bereits verwendeter Farben.
- [x] Implementieren ein System zur Platzierung von Punkten per Mausklick mit automatischer Verknüpfung zum Raster und Paarung von Punkten gleicher Farbe.

Um heute zu beginnen, habe ich mir Gedanken darüber gemacht, wie ich dieses Toolkit zur Erstellung von Levels implementieren könnte. Zuerst dachte ich daran, etwas Neues auszuprobieren, eine neue Programmiersprache oder eine neue Bibliothek in den Sprachen, die ich bereits kenne, aber schnell genug kam ich zu dem Schluss, dass es viel einfacher und effizienter ist, es in Unity zu implementieren. Zuerst dachte ich daran, ein separates Projekt zu erstellen und .exe zu verwenden, um LevelData-Dateien für das Hauptspiel zu erstellen, aber schnell genug wurde mir klar, dass es eine viel bessere Option gibt, nämlich einen Editor direkt im Projekt mit dem Spiel. Es stellt sich heraus, dass Unity bereits eine Option hat, um Ihre eigenen Editoren für mehr Komfort zu erstellen, die in das obere Unity-Fenster passt. In der Tat, das ist, was ich am Ende zu tun. Zuerst habe ich das Fenster selbst implementiert und dann die Farbpalette und das Raster selbst hinzugefügt. Dann fügte ich die Möglichkeit hinzu, die Grösse des Rasters zu bearbeiten und Punkte auf dem Raster zu platzieren. Damit habe ich bereits die Grundfunktionalität für die Erstellung von Levels, allerdings ohne die Möglichkeit, sie in die LevelData-Datei zu exportieren.


## 17.01
- [x] Aktualisieren die Palette, indem absolut alle Farben hinzufügen, die im Spiel verwendet werden.
- [x] Fügen einen Testmodus hinzu, der das Spielgeschehen vollständig emuliert und die Möglichkeit bietet, Linien zwischen gleichfarbigen Punkten nach den Spielregeln zu ziehen.
- [x] Erstellung eines Systems zum Speichern und Anzeigen von Linien mit der Möglichkeit, die letzte Aktion abzubrechen und Überschneidungen zu verhindern
- [x] Implementieren die Mechanik des Löschens bestehender Linien, wenn Sie mit dem Zeichnen einer neuen Linie der gleichen Farbe beginnen.

Heute habe ich damit begonnen, die Basisfarbpalette durch eine Palette zu ersetzen, die alle im Spiel verwendeten Farben enthält. Als Nächstes habe ich mich daran gemacht, einen Testmodus direkt in den Editor einzubauen, damit ich jeden Level, den ich erstellt habe, bequem testen kann, bevor ich ihn speichere, was mir die meisten Schwierigkeiten bereitet hat. Es war schwierig, die Grundfunktionalität des Spiels neu zu implementieren, weil ich vergessen hatte, wie ich es im Hauptspiel gemacht hatte, ich musste eine Menge Dinge korrigieren und es ist immer noch nicht in perfektem Zustand, aber ich habe beschlossen, dass diese Qualität für den Editor ganz gut ist. Parallel zur Implementierung des Gameplays wiederholte ich auch das in LineManager implementierte System zum Speichern und Anzeigen von Linien, wiederum direkt im Editor. Und am Ende habe ich die Mechanik des Löschens von Linien zu Beginn des Zeichnens einer neuen Linie implementiert.


## 24.01
- [ ] Hinzufügen der Funktionalität des Level-Exports zu ScriptableObject mit automatischer Nummerierung und Speicherung in einem Ordner.
- [ ] Hinzufügen der Möglichkeit, einen bestehenden Level über die Dropdown-Liste aller Dateien zu laden, um ihn zu bearbeiten.
- [ ] Entfernen die Funktion zur Erstellung von Hinweisen aus dem Hauptspiel
- [ ] Implementierung der Funktion zur Erstellung von Hinweisen direkt im Editor und der Möglichkeit, diese zu ändern

...


## 31.01
- [ ] ...
- [ ] ...
- [ ] ...
- [ ] ...

...


# Reflexion

...
