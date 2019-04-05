using RechnerInterface;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TestRechner
{
    public class TemplateTestRechnerImplementation : IRechnung
    {
        /// <summary>
        /// Diese Method soll eine Kettenrechnung durchführen.
        /// Wenn z.B. der String "2 + 3 + 55" gegeben ist, so muss die Komponente ohne Beachtung von Punkt- vor Strich-Rechnung
        /// einfach die Zahlen von links nach rechts mit den Operatoren verknüpfen.
        /// Also wäre hier das Ergebnis 2 + 3 = 5; 5 + 55 = 60; Das Endergebnis wäre 60.
        /// 
        /// Das Endergebnis ist an das Finished Event zu schicken. Für alle Zwischenergebnisse soll ein ProgressChanged Event geschickt werden. 
        ///
        /// Der Code soll keine Fehler auslösen. 
        /// 
        /// Syntaktische Fehler in der Zeichenfolge sind nicht erlaubt und müssen erkannt werden, d.h. fehlende oder zu viele Leerzeichen oder 
        /// ungültige Zeichen dürfen nicht vorkommen.
        /// 
        /// Im Fehlerfall wird nur das Finished Event ausgelöst und die Verarbeitung beendet.
        /// 
        /// Es wird nur ganzzahlige Division ausgeführt.
        /// </summary>
        /// <param name="rechnung">"3 + 5 / 7" => 1 </param>
        public void Rechne(string rechnung)
        {
            var regexPattern = new Regex("[0-9/*-+]+$");
            var ergebnis = new int?();
            var operand_1 = new int?();
            int operand_2;
            var eingabeZeichen = "";
            char oparator = ' ';

            var eingabe = rechnung
                .ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray();

            if (!eingabe.Any() || eingabe.All(c => Char.IsWhiteSpace(c)))
            {
                Finished(this, new CalculationStatusEventArgs(0, "Die Eingabe ist leer."));
                return;
            }

            if (!regexPattern.IsMatch(rechnung))
            {
                Finished(this, new CalculationStatusEventArgs(-1, "Ein nicht gültiges Zeichen wurde verwendet."));
                return;
            }

            for (int index = 0; index < eingabe.Length; index++)
            {
                var zeichen = eingabe[index];
                int zahl;

                if (int.TryParse(zeichen.ToString(), out zahl))
                {
                    eingabeZeichen += zeichen.ToString();
                    if(index < eingabe.Length-1)
                        continue;
                }

                if (!operand_1.HasValue)
                {
                    operand_1 = int.Parse(eingabeZeichen);
                    eingabeZeichen = "";

                    if (!ergebnis.HasValue)
                    {
                        ergebnis = operand_1;
                        ProgressChanged(this, new CalculationStatusEventArgs(operand_1.Value, $"Set: {operand_1}"));
                    }                   
                }

                if (oparator == ' ')
                {
                    oparator = zeichen;
                    continue;
                }

                if (!int.TryParse(eingabeZeichen, out zahl))
                {
                    Finished(this, new CalculationStatusEventArgs(-1, "Ein nicht gültiges Zeichen wurde verwendet."));
                    return;
                }

                operand_2 = int.Parse(eingabeZeichen);
                eingabeZeichen = "";

                switch (oparator)
                {
                    case '+':
                        operand_1 = Addiere(operand_1.Value, operand_2);
                        break;

                    case '-':
                        operand_1 = Subrahiere(operand_1.Value, operand_2);
                        break;

                    case '*':
                        operand_1 = Multipliziere(operand_1.Value, operand_2);
                        break;

                    case '/':
                        if (operand_2 == 0)
                        {
                            Finished(this, new CalculationStatusEventArgs(-1, "Division durch 0 nicht möglich."));
                            return;
                        }
                        operand_1 = Dividiere(operand_1.Value, operand_2);
                        break;

                    default:
                        Finished(this, new CalculationStatusEventArgs(-1, "Ein nicht gültiges Zeichen wurde verwendet."));
                        return;
                }

                ergebnis = operand_1;
                oparator = zeichen;
            }

            Finished(this, new CalculationStatusEventArgs(ergebnis??0, $"Finished: {ergebnis}"));
        }

        public event EventHandler<CalculationStatusEventArgs> ProgressChanged;

        public event EventHandler<CalculationStatusEventArgs> Finished;

        private int Addiere(int aktuellerWert, int summand)
        {
            var ergebnis = aktuellerWert + summand;
            ProgressChanged(this, new CalculationStatusEventArgs(ergebnis, $"Plus: {summand}"));
            return ergebnis;
        }

        private int Subrahiere(int aktuellerWert, int subtrahend)
        {
            var ergebnis = aktuellerWert - subtrahend;
            ProgressChanged(this, new CalculationStatusEventArgs(ergebnis, $"Minus: {subtrahend}"));
            return ergebnis;
        }

        private int Multipliziere(int aktuellerWert, int faktor)
        {
            var ergebnis = aktuellerWert * faktor;
            ProgressChanged(this, new CalculationStatusEventArgs(ergebnis, $"Multipliziert: {faktor}"));
            return ergebnis;
        }

        private int Dividiere(int aktuellerWert, int divisor)
        {
            var ergebnis = aktuellerWert / divisor;
            ProgressChanged(this, new CalculationStatusEventArgs(ergebnis, $"Dividiert: {divisor}"));
            return ergebnis;
        }
    }
}
