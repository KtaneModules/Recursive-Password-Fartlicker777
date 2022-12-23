using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;
using KModkit;

public class RecursivePassword : MonoBehaviour {
   public KMBombModule Module;
   public KMBombInfo BombInfo;
   public KMAudio Audio;

   public KMSelectable ToggleSel;
   public KMSelectable[] ArrowSels;
   public TextMesh[] ScreenTexts;

   private int _moduleId;
   private static int _moduleIdCounter = 1;
   private bool _moduleSolved;

   bool Animating;

   private int[] _currentLetters = new int[5];
   int[] SelectedWords = new int[5];
   int Password;
   int[] ColSwap = new int[5];

   int[] SubmitIndicies = new int[5];

   string ScrambledPassword = "";

   bool SubmitMode;

   string[] WordList = { "AGAPE", "ABYSS", "BELOW", "BEIGE", "CHILL", "CHAMP", "DIZZY", "DEVIL", "EQUAL", "EJECT", "FJORD", "FLAME", "GHOUL", "GANKS", "HOCUS", "HEART", "INEPT", "INGOT", "JIVES", "JOKED", "KARMA", "KNIFE", "LIGMA", "LOBED", "MISTY", "MODAL", "NONCE", "NOOSE", "OBEYS", "OTAKU", "PIMPS", "PRAYS", "QUIET", "QUONK", "REPEL", "RAZED", "SIREN", "SHREW", "TOWED", "TOPAZ", "UNJAM", "UNZIP", "VEILS", "VIXEN", "WAGER", "WHELK", "XYSTI", "XENON", "YAWNS", "YEAST", "ZONKS", "ZONED" };

   string[][] LetterGrid = new string[][] {
       new string[] {"", "", "", "", ""},
       new string[] {"", "", "", "", ""},
       new string[] {"", "", "", "", ""},
       new string[] {"", "", "", "", ""},
       new string[] {"", "", "", "", ""}
      };

   private void Start () {
      _moduleId = _moduleIdCounter++;
      ToggleSel.OnInteract += TogglePress;
      for (int i = 0; i < ArrowSels.Length; i++) {
         ArrowSels[i].OnInteract += ArrowPress(i);
      }

      Generate();

      //_currentLetters = Enumerable.Range(0, 26).ToArray().Shuffle().Take(5).ToArray();
      UpdateScreens();
   }

   void Generate () {
      Reroll:
      Password = Rnd.Range(0, 52);
      //bool check = false;

      char[] Letters = WordList[Password].ToCharArray();
      List<string> BadLetters = new List<string> { };

      //Debug.Log(Letters[0]);

      for (int i = 0; i < 5; i++) {
         do {
            SelectedWords[i] = Rnd.Range(0, 52);
         } while (!WordList[SelectedWords[i]].Substring(1).Contains(Letters[i])); //Finds a random word with a common letter as the current one.

         List<int> Possibilities = new List<int> { };

         for (int j = 1; j < 5; j++) {
            if (WordList[SelectedWords[i]][j] == Letters[i]) { //If there's a duplicate letter, choose a random position.
               Possibilities.Add(j);
            }
         }
         Possibilities.Shuffle();
         ColSwap[i] = Possibilities[0]; //Remember what column that letter is.
         BadLetters.Add(WordList[SelectedWords[i]][Possibilities[0]].ToString());
      }

      for (int i = 0; i < 4; i++) {
         for (int j = i + 1; j < 5; j++) {
            if (SelectedWords[i] == SelectedWords[j]) { //Reused word
               goto Reroll;
            }
         }
      }
      for (int i = 0; i < 5; i++) {
         if (SelectedWords[i] == Password) {
            goto Reroll;
         }
      }

      for (int i = 0; i < 5; i++) {
         _currentLetters[i] = Rnd.Range(0, 5); //Shuffles each column starting position
      }

      for (int i = 0; i < 5; i++) {
         for (int j = 0; j < 5; j++) {
            if (ColSwap[i] == j) {
               string OldChar = WordList[SelectedWords[i]][j].ToString();
               do {
                  LetterGrid[i][j] = "abcdefghijklmnopqrstuvwxyz"[Rnd.Range(0, 26)].ToString();
               } while (OldChar.ToLower() == LetterGrid[i][j] || BadLetters.Contains(LetterGrid[i][j])); //Makes up a random fake letter to replace
            }
            else {
               LetterGrid[i][j] = WordList[SelectedWords[i]][j].ToString();
            }
         }
      }

      Debug.LogFormat("[Recursive Password #{0}] The letters on the module are:", _moduleId);
      Debug.LogFormat("[Recursive Password #{0}] {1}", _moduleId, LetterGrid[0][0] + LetterGrid[0][1] + LetterGrid[0][2] + LetterGrid[0][3] + LetterGrid[0][4]);
      Debug.LogFormat("[Recursive Password #{0}] {1}", _moduleId, LetterGrid[1][0] + LetterGrid[1][1] + LetterGrid[1][2] + LetterGrid[1][3] + LetterGrid[1][4]);
      Debug.LogFormat("[Recursive Password #{0}] {1}", _moduleId, LetterGrid[2][0] + LetterGrid[2][1] + LetterGrid[2][2] + LetterGrid[2][3] + LetterGrid[2][4]);
      Debug.LogFormat("[Recursive Password #{0}] {1}", _moduleId, LetterGrid[3][0] + LetterGrid[3][1] + LetterGrid[3][2] + LetterGrid[3][3] + LetterGrid[3][4]);
      Debug.LogFormat("[Recursive Password #{0}] {1}", _moduleId, LetterGrid[4][0] + LetterGrid[4][1] + LetterGrid[4][2] + LetterGrid[4][3] + LetterGrid[4][4]);

      Debug.LogFormat("[Recursive Password #{0}] The selected words are {1}, {2}, {3}, {4}, {5}", _moduleId, WordList[SelectedWords[0]], WordList[SelectedWords[1]], WordList[SelectedWords[2]], WordList[SelectedWords[3]], WordList[SelectedWords[4]]);

      Debug.LogFormat("[Recursive Password #{0}] The replaced letters make the word \"{1}\"", _moduleId, WordList[Password]);

      //Debug.Log(WordList[Password]);
   }

   private KMSelectable.OnInteractHandler ArrowPress (int i) {
      return delegate () {
         ArrowSels[i].AddInteractionPunch(0.25f);
         Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, ArrowSels[i].transform);
         if (_moduleSolved || Animating) {
            return false;
         }
         if (!SubmitMode) {
            if (i < 5) {
               _currentLetters[i] = (_currentLetters[i] + 1) % 5; //Normal button press things.
            }
            else {
               _currentLetters[i % 5]--;
               if (_currentLetters[i % 5] == -1) {
                  _currentLetters[i % 5] = 4;
               }
            }
         }
         else {
            if (i < 5) {
               SubmitIndicies[i] = (SubmitIndicies[i] + 1) % 26;
            }
            else {
               SubmitIndicies[i % 5]--;
               if (SubmitIndicies[i % 5] == -1) {
                  SubmitIndicies[i % 5] = 25;
               }
            }
         }


         UpdateScreens();

         // Do things

         return false;
      };
   }

   private void UpdateScreens () {
      if (!SubmitMode) {
         for (int i = 0; i < 5; i++) {
            ScreenTexts[i].text = LetterGrid[_currentLetters[i]][i].ToString().ToUpper();
         }
      }
      else {
         for (int i = 0; i < 5; i++) {
            ScreenTexts[i].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[SubmitIndicies[i]].ToString();
         }
      }
   }

   void GoIntoSubmitMode () {
      UpdateScreens();
   }

   IEnumerator Solve () {
      Animating = true;
      Audio.PlaySoundAtTransform("Good", transform);
      ScreenTexts[0].text = "G";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[1].text = "R";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[2].text = "E";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[3].text = "A";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[4].text = "T";
      yield return new WaitForSeconds(.3f);
      ScreenTexts[0].text = "S";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[1].text = "O";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[2].text = "L";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[3].text = "V";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[4].text = "E";
      yield return new WaitForSeconds(.45f);
      for (int i = 0; i < 5; i++) {
         ScreenTexts[i].text = "";
      }
      yield return new WaitForSeconds(.13f);
      for (int i = 0; i < 5; i++) {
         ScreenTexts[i].text = "!";
      }
      GetComponent<KMBombModule>().HandlePass();
   }

   IEnumerator Strike () {
      Animating = true;
      Audio.PlaySoundAtTransform("Bad", transform);
      ScreenTexts[0].text = "S";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[1].text = "K";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[2].text = "I";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[3].text = "L";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[4].text = "L";
      yield return new WaitForSeconds(.3f);
      ScreenTexts[0].text = "I";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[1].text = "S";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[2].text = "S";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[3].text = "U";
      yield return new WaitForSeconds(.1f);
      ScreenTexts[4].text = "E";
      yield return new WaitForSeconds(.58f);
      /*for (int i = 0; i < 5; i++) {
         ScreenTexts[i].text = "?";
      }*/
      //yield return new WaitForSeconds(.13f);
      GetComponent<KMBombModule>().HandleStrike();
      //yield return new WaitForSeconds(.13f);
      Animating = false;
      SubmitMode = false;
      for (int i = 0; i < 5; i++) {
         SubmitIndicies[i] = 0;
      }
      UpdateScreens();
   }


   private bool TogglePress () {
      if (Animating) {
         return false;
      }
      ToggleSel.AddInteractionPunch(0.5f);
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ToggleSel.transform);
      if (_moduleSolved) {
         return false;
      }

      if (!SubmitMode) {
         SubmitMode = true;
         GoIntoSubmitMode();
      }
      else {
         if (ScreenTexts[0].text + ScreenTexts[1].text + ScreenTexts[2].text + ScreenTexts[3].text + ScreenTexts[4].text == WordList[Password]) {
            StartCoroutine(Solve());
         }
         else {
            StartCoroutine(Strike());
         }
      }

      return false;
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"!{0} cycle to cycle the screens. Use !{0} toggle to toggle the mod. Use !{0} XXXXX to submit a word.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Command = Command.Trim().ToUpper();
      yield return null;
      if (Command == "TOGGLE") {
         ToggleSel.OnInteract();
      }
      else if (Command == "CYCLE") {
         for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
               ArrowSels[i].OnInteract();
               yield return new WaitForSeconds(.2f);
            }
         }
      }
      else {
         if (Regex.Match(Command, "^([a-zA-Z]){5}$").Success && SubmitMode) {
            for (int i = 0; i < 5; i++) {
               while (ScreenTexts[i].text != Command[i].ToString()) {
                  ArrowSels[i].OnInteract();
                  yield return new WaitForSeconds(.1f);
               }
            }
            ToggleSel.OnInteract();
         }
         else {
            yield return "sendtochaterror I don't understand!";
         }
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return ProcessTwitchCommand("toggle");
      yield return new WaitForSeconds(.1f);
      yield return ProcessTwitchCommand (WordList[Password]);
   }
}
