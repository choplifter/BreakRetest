# Dokumentation: Break-Retest Pro Trading Indikator (NinjaTrader 8)

## 1. Einleitung & Trading-Konzept

Das **Break-Retest-Setup** gehört zu den verlässlichsten und am häufigsten gehandelten Mustern im institutionellen und privaten Trading. Es basiert auf dem Prinzip der Marktstruktur: Sobald ein signifikantes Unterstützungs- oder Widerstandslevel mit Dynamik durchbrochen wird, wandelt sich die Funktion dieses Levels (Rollenwechsel/Polarität). Ein ehemaliger Widerstand wird zur Unterstützung und umgekehrt.

Viele algorithmische Ansätze scheitern an zwei Kernproblemen des Marktes:
1. **Marktrauschen:** Märkte drehen selten auf den exakten Tick genau an einer historischen Linie. Sie neigen dazu, Liquidität kurz hinter Levels abzugreifen (Stop Hunting).
2. **Fehlausbrüche (Fakeouts):** Ausbrüche ohne echtes institutionelles Interesse (Volumen) kehren schnell wieder um.

### Der Paradigmenwechsel: Zonen & State-Machine
Dieser Indikator löst diese Probleme durch ein professionelles Software-Design:
* **Denken in Zonen statt Linien:** Jedes identifizierte Level wird automatisch mit einer mathematischen Toleranz-Zone (Puffer) versehen.
* **Zustandsbasierte Logik (State-Machine):** Ein Level wird nicht als einfaches Ereignis betrachtet, sondern durchläuft einen autonomen Lebenszyklus. Dies verhindert ein Chaos aus unübersichtlichen Variablen (`bool`-Flags) und macht das Setup präzise überwachbar.

---

## 2. Die State-Machine (Lebenszyklus einer Zone)

Jede generierte Zone besitzt einen Zustand (`ZoneState`), der sich basierend auf der Preisbewegung dynamisch verändert:

1. **`Active` (Aktiv):** Das Level wurde frisch im Markt gebildet (z. B. ein neues Swing High). Der Preis befindet sich noch unterhalb (Widerstand) oder oberhalb (Unterstützung) der Zone.
2. **`Broken` (Ausgebrochen):** Eine Kerze schließt klar außerhalb der definierten Toleranzgrenze. Der Ausbruch ist somit mathematisch bestätigt. Die Zone wechselt in den Beobachtungsmodus für einen potenziellen Rücklauf.
3. **`Retesting` (Im Test):** Der Preis korrigiert und taucht in die Toleranz-Zone des durchbrochenen Levels ein. Die State-Machine signalisiert höchste Aufmerksamkeit – der Markt ist am "Point of Interest".
4. **`Confirmed` (Bestätigt / Einstieg):** Der Preis zeigt innerhalb der Zone eine vordefinierte bullishe oder bearishe Reaktion (Trigger). Ein Kaufs- oder Verkaufs-Signal wird generiert.
5. **`Invalidated` (Ungültig):** Der Preis bricht beim Retest komplett in die Gegenrichtung durch (z. B. Schlusskurs wieder tief unter dem alten Widerstand). Das Setup wird als Fehlausbruch verworfen.

---

## 3. Level-Generierung & Marktkontext

Ein funktionierendes Break-Retest-System steht und fällt mit der Qualität der zugrundeliegenden Levels. Der Indikator kombiniert zwei komplementäre Ansätze:

### A. High-Timeframe Kontext: Vortages-Levels (Prior Day OHLC)
Große Marktteilnehmer (Algos, Banken) orientieren sich primär an übergeordneten Marken. Der Indikator berechnet zu Beginn jeder neuen Handelssession automatisch:
* **Previous Day High (PDH):** Das absolute Hoch des Vortages. Ein Ausbruch hierüber signalisiert starkes bullishes Momentum.
* **Previous Day Low (PDL):** Das absolute Tief des Vortages. Ein Ausbruch darunter signalisiert bearishen Verkaufsdruck.

Diese Zonen werden auf dem Chart farblich hervorgehoben (**Dunkelrot** für Widerstand / **Dunkelgrün** für Unterstützung), da sie eine deutlich höhere statistische Relevanz aufweisen als untergeordnete Price-Action-Swings.

### B. Mikro-Struktur: Swing Highs / Swing Lows (Pivots)
Für fortlaufende Trading-Gelegenheiten scannt der Indikator die Price Action nach lokalen Extrempunkten. Ein Level wird über eine **N-Bar-Regel** definiert:
* Ein **Swing High** liegt vor, wenn eine Kerze links und rechts von jeweils *N* Kerzen flankiert wird, deren Hochs tiefer liegen.
* Ein **Swing Low** liegt vor, wenn eine Kerze links und rechts von jeweils *N* Kerzen flankiert wird, deren Tiefs höher liegen.

*N* wird über den Parameter **`Swing Strength`** gesteuert. Diese Zonen werden etwas dezenter gezeichnet (**Hellrot** / **Hellgrün**).

---

## 4. Modulare Trigger-Logiken & Volumen-Filter

Sobald eine Zone den Zustand `Retesting` erreicht, entscheidet die ausgewählte Trigger-Komponente über den Einstieg. Im Indikator-Menü stehen drei hochentwickelte Logiken zur Auswahl:

### 1. Simple Price Action
* **Logik:** Der Trigger feuert sofort, wenn der Schlusskurs der aktuellen Kerze in Ausbruchsrichtung schließt und sich wieder leicht außerhalb der Zone befindet (z. B. grüne Kerze schließt über der Oberkante des ehemaligen Widerstands).
* **Eignung:** Sehr aggressiver Einstieg. Bietet den frühestmöglichen Einstieg, birgt jedoch das höchste Risiko für "whipsaws" (Sägezahnbewegungen).

### 2. Engulfing Pattern
* **Logik:** Wartet auf eine klassische Candlestick-Umkehrformation direkt in der Zone. Ein *Bullish Engulfing* liegt vor, wenn der Körper der aktuellen grünen Kerze den Körper der vorherigen roten Kerze vollständig umschließt.
* **Eignung:** Konservativer Price-Action-Einstieg. Bestätigt, dass die Gegenbewegung der Korrektur bereits begonnen hat.

### 3. Volume-Backed Engulfing (Institutioneller Filter)
* **Logik:** Dies ist die professionellste Variante. Sie kombiniert das *Engulfing Pattern* mit einer algorithmischen Volumen-Analyse. Bei jedem potenziellen Signal berechnet der Indikator das Durchschnittsvolumen der letzten 20 Kerzen ($V_{avg}$). Das Signal wird **nur dann freigegeben**, wenn das Volumen der Einstiegskerze ($V_{0}$) signifikant über dem Durchschnitt liegt:
  $$V_{0} > V_{avg} 	imes 1.2$$
* **Eignung:** Filtert aktiv volumenschwache Bewegungen (z. B. während der Mittagszeit oder bei Feiertagen) heraus. Ein Signal mit einem **Volumen-Spike (20%+)** zeigt, dass institutionelles Kapital aktiv bereitsteht, das Level zu verteidigen. Diese Signale werden auf dem Chart zusätzlich mit dem Text **"VOL"** markiert.

---

## 5. Parameter-Leitfaden (Settings-Menü)

Nach dem Laden des Indikators in NinjaTrader 8 können Sie folgende Parameter über das Eigenschaften-Fenster anpassen:

| Parameter | Datentyp | Standardwert | Beschreibung |
| :--- | :--- | :--- | :--- |
| **`Swing Strength`** | Integer | `5` | Bestimmt die Validität der Mikro-Swings. Ein Wert von 5 bedeutet, dass ein Hoch/Tief 5 Kerzen links und rechts überragen muss. Höhere Werte = wichtigere Level, aber weniger Signale. |
| **`Zone Tolerance (Ticks)`** | Integer | `4` | Die Dicke der Zone in Ticks (nach oben und unten vom exakten Level). Bei stark volatilen Märkten (z. B. Nasdaq-Future / NQ) sollte dieser Wert erhöht werden (z. B. 8-12 Ticks). |
| **`Use Prior Day Levels`** | Boolean | `true` | Schaltet das automatische Zeichnen und Berechnen von Vortages-Hochs und Vortages-Tiefs ein/aus. |
| **`Trigger Logic`** | Enum | `VolumeBackedEngulfing` | Auswahl des Einstiegs-Algorithmus: `SimplePriceAction`, `Engulfing` oder `VolumeBackedEngulfing`. |

---

## 6. Code-Architektur & C# Implementierung

Für Entwickler bietet das Skript eine saubere, objektorientierte C#-Struktur unter Verwendung von Interfaces (SOLID-Prinzipien):

### Kernkomponenten des Codes:
* **`enum ZoneState` / `enum ZoneType`:** Kapseln die Zustände und Richtungen global, sodass sie im gesamten NinjaTrader-Ökosystem (inkl. Strategie-Builder und Market Analyzer) ohne Namespace-Konflikte aufgerufen werden können.
* **`class TradingZone`:** Repräsentiert eine autonome Zone. Sie berechnet ihre eigenen mathematischen Grenzen (`UpperBound`, `LowerBound`) basierend auf der Tick-Größe des zugrunde liegenden Instruments (`TickSize`) und verwaltet ihren aktuellen Bar-Index für das Zeichnen von Rechtecken.
* **`interface ITriggerCondition`:** Ermöglicht polymorphes Verhalten. Die Klassen `SimplePriceActionTrigger`, `EngulfingTrigger` und `VolumeBackedEngulfingTrigger` implementieren dieses Interface. Dadurch kann die Einstiegslogik zur Laufzeit ausgetauscht werden, ohne den Kernalgorithmus der State-Machine zu verändern.
* **`OnBarUpdate()` (Haupt-Loop):**
  1. Filtert die ersten 21 Bars (historischer Puffer für die Volumen-Gleitenden-Durchschnitte).
  2. Überprüft bei `Bars.IsFirstBarOfSession` neue Vortages-Marken über den nativen `PriorDayOHLC()`-Indikator.
  3. Ruft `DetectSwingLevels()` für neue Mikro-Strukturen auf.
  4. Iteriert in `ProcessZones()` über alle aktiven Zonen und aktualisiert deren States sowie die visuelle Darstellung mittels `Draw.Rectangle`.
  5. Nutzt ein ressourcenschonendes Speichermanagement via `RemoveAll()`, um geschlossene oder ungültige Zonen aus der aktiven Überwachungsschleife zu entfernen.

---

## 7. Installations- & Setup-Anleitung in NinjaTrader 8

Um den Indikator in Ihrem NinjaTrader zu aktivieren, folgen Sie diesen Schritten:

1. Öffnen Sie NinjaTrader 8.
2. Wählen Sie im oberen Hauptmenü: **New** $ightarrow$ **NinjaScript Editor**.
3. Klicken Sie im NinjaScript Editor auf der rechten Seite im *NinjaScript Explorer* mit der rechten Maustaste auf den Ordner **Indicators** und wählen Sie **New Indicator...**
4. Vergeben Sie als Namen exakt: **`BreakRetest`** (Achtung auf Groß-/Kleinschreibung!). Klicken Sie auf *Next* und anschließend auf *Finish*.
5. Löschen Sie im sich öffnenden Code-Fenster den **gesamten** automatisch generierten Text (`Strg + A` drücken, dann `Entf`). Das Fenster muss komplett leer sein (keine einzige Zeile darf übrig bleiben).
6. Kopieren Sie den bereitgestellten, fehlerbereinigten V3-Quellcode vollständig.
7. Fügen Sie den Code in das leere Editor-Fenster ein (`Strg + V`).
8. Drücken Sie die Taste **`F5`** auf Ihrer Tastatur (oder klicken Sie oben in der Symbolleiste auf das grüne **Compile**-Symbol).
9. Unten im Editor erscheint kurz ein grauer Ladebalken. Sobald das akustische Erfolgs-Signal ertönt, ist der Indikator einsatzbereit.
10. Öffnen Sie einen beliebigen Chart (z. B. Micro E-mini S&P 500 / MES), machen Sie einen Rechtsklick in den Chart, wählen Sie **Indicators**, fügen Sie **`BreakRetest`** hinzu und klicken Sie auf OK.

---

## 8. Professionelle Trading-Anwendung & Risikomanagement

Ein Indikator liefert Signale, aber ein Trader managt das Risiko. Für den profitablen Handel des Break-Retest-Musters sollten Sie folgende Regeln beachten:

### A. Platzierung von Stop Loss & Take Profit
* **Long-Setup (Kauf):** Der grüne Pfeil signalisiert den Einstieg nach dem Retest einer Unterstützung (oder eines alten Widerstands). Der **Stop Loss (SL)** gehört knapp unter das absolute Tief, welches *während* des `Retesting`-Zustands innerhalb der Zone geformt wurde (zzgl. 1-2 Ticks Puffer). Das **Take Profit (TP)** orientiert sich am jüngsten Swing High (dem Ausbruchshoch).
* **Short-Setup (Verkauf):** Der rote Pfeil signalisiert den Short-Einstieg. Der SL gehört knapp über das höchste Hoch des Retests. Das TP liegt am jüngsten Swing Low.

### B. Das CRV (Chance-Risiko-Verhältnis)
Da der Indikator durch das Warten auf den Retest sehr nah am Ursprung der Bewegung einsteigt, ist das CRV bei diesem Setup von Natur aus exzellent. Suchen Sie nach Trades, die ein **CRV von mindestens 1:2** aufweisen (der potenzielle Gewinn ist doppelt so groß wie das Risiko).

### C. Der übergeordnete Trend-Filter
Der Indikator zeichnet sowohl Long- als auch Short-Signale. Handeln Sie idealerweise **nur in Richtung des übergeordneten Trends**. 
* Befindet sich der Markt übergeordnet (z. B. im 1-Stunden-Chart) in einem starken Aufwärtstrend, filtern Sie die roten Short-Signale im 5-Minuten-Chart manuell heraus und handeln Sie **ausschließlich die grünen Long-Signale**.
