# Turing's paper machine

The goal of this project is to reconstruct the first chess-playing program, created by Alan Turing and David Champernowne in 1948, many years before the first computer capable of running it was built.
Most notably, the entire program is contained in a single file (`Chess-Challenge/src/My Bot/MyBot.cs`), with a size limit of 1024 tokens (as defined in Sebastian Lague's chess coding challenge).

The algorithm, known as *Turochamp* or *Turing's paper machine*, is considered to be the first computer game ever created, even though it was never actually run on a computer.
For each move, it reportedly took Turing 15 to 20 minutes to compute the result by hand (with a depth of 3 ply).

There are some open-source implementations of *Turochamp* available on [GitHub](https://github.com/topics/turochamp), as well as the original closed-source recreation by the ChessBase team, which was used as a reference for this reverse-engineering project due to being the closest to the original algorithm.

## Implementation notes

- Even though the Alpha-Beta search algorithm wasn't invented until 1958, 10 years after *Turochamp*, it is used in this implementation since it does not affect the algorithm's behavior and is strictly more efficient than a naive minimax search. Furthermore, there is evidence that Turing did not bother to explore moves that were obviously bad, so he was intuitively using a form of pruning.

- Quiescence search is used to avoid the horizon effect. This improvement was already present in the original *Turochamp* algorithm.

- Rule 7 of the original algorithm (*"Add 1.0 point for the threat of mate and 0.5 point for a check."*) is not implemented, since it doesn't make sense in the context of a modern minimax or Alpha-Beta framework. The existing open-source implementations of *Turochamp* also do not agree on how to interpret this rule and most of them ignore it or implement it incorrectly.

- Rule 5 is also ambiguous: *"Add 1.0 point for the possibility of still being able to castle on a later move if a King or Rook move is being considered; add another point if castling can take place on the next move; finally add one more point for actually castling."*.

  The main parts that don't make sense in a minimax algorithm are:
  
  - *"if a King or Rook **move** is being considered"*: the minimax/alphabeta search evaluates **positions** at each leaf node, without knowing which **moves** took us there (this is the reason transposition tables are even possible).
  - *"add one more point for **actually castling**"*: again, we are evaluating positions, not moves. [Artificial castling](https://en.wikipedia.org/wiki/Castling#Artificial_castling) is equally valid and should not be penalized. Also, when the game starts from a FEN position, we literally can't know if we actually castled or just moved the king and rook.
  
  Since it doesn't make sense to add points based on the moves that led to the current position, I decided to interpret this rule as follows:
  
  - First, all the other positional rules (except 7) are computed on each leaf node for both the player (positive score) and the opponent (negative score).
  - The material score is also added on each leaf node for both players, as is standard in a minimax/alphabeta search.
  - Then, use alpha-beta to score each top-level move (i.e. each move of the root node).
  - Before returning the best move, apply extra points to the moves according to Rule 5.
  
  In other words, castling incentives are applied *only at the root node*, in order to slightly boost some top-level moves.
  
  *Note:* The existing implementations seem to agree that, for a castling move, the extra points do stack (i.e. a castling move is awarded 3 points, even though we can't castle on the next move or any future moves). This seems like the most reasonable interpretation, even though the wording of the rule is very ambiguous.

## Strategies used to reduce code size

- Use `var` instead of explicit type declarations for generic types.

- Use closures (lambdas, anonymous functions) to minimize the number of arguments passed to functions.

## References

[F. Friedel, G. Kasparov. *Reconstructing Turing's "Paper Machine"*. ChessBase](https://en.chessbase.com/post/reconstructing-turing-s-paper-machine)

[*Turochamp*. Wikipedia, the free encyclopedia](https://en.wikipedia.org/wiki/Turochamp)

---

# Original README.md

# Chess Coding Challenge (C#)
Welcome to the [chess coding challenge](https://youtu.be/iScy18pVR58)! This is a friendly competition in which your goal is to create a small chess bot (in C#) using the framework provided in this repository.
Once submissions close, these bots will battle it out to discover which bot is best!

I will then create a video exploring the implementations of the best and most unique/interesting bots.
I also plan to make a small game that features these most interesting/challenging entries, so that everyone can try playing against them.

## Submission Due Date
October 1st 2023.<br>
Entries can be submitted over [here](https://forms.gle/6jjj8jxNQ5Ln53ie6).<br>
You are free to edit your entry at any point up to the due date.

## Change Log
It has been necessary to make some bug fixes to the original project, and I've also been tempted (by some great suggestions from the community) into making a few non-breaking improvements/additions to the API. I realize that changes can be frustrating during a challenge though, and so will commit to freezing the API from August 1st.

* <b>V1.1</b> Fixed major bug affecting `board.GetPiece()` and `PieceList` functions. Added `Board.CreateBoardFromFEN()`.
* <b>V1.11</b> UI changes: Added coordinate names to board UI and fixed human player input bug.
* <b>V1.12</b> Small fixes to `board.IsDraw()`: Fifty move counter is now updated properly during search, and insufficient material is now detected for lone bishops on the same square colour.
* <b>V1.13</b> Fixed issue with `board.ZobristKey` where value would sometimes be different after making and undoing a move. Added an alternative function for getting moves `board.GetLegalMovesNonAlloc()` (see docs for more info).
* <b>V1.14</b> A handful of additions to the Board API: `board.IsInsufficientMaterial()`, `board.IsRepeatedPosition()`, `board.GameRepetitionHistory`, `board.FiftyMoveCounter`, `board.GameMoveHistory`, `board.GameStartFenString`.
* <b>V1.15</b> Fixed incorrect `move.CapturePieceType` for en-passant moves and moves in `board.GameMoveHistory`. Added `BitboardHelper.VisualizeBitboard()` to help with debugging bitboards.
* <b>V1.16</b> Added `timer.GameStartTimeMilliseconds`, `timer.OpponentMillisecondsRemaining`, `board.ForceSkipTurn()`.
* <b>V1.17</b> Added `BitboardHelper.GetPieceAttacks()` and optimized `board.SquareIsAttackedByOponent()`. Writing `#DEBUG` in a comment will now exclude code in that line from counting towards the token limit (for testing only of course).
* <b>V1.18</b> Added `timer.IncrementMilliseconds` (this will be 0 for the main tournament, but a small increment may be used in the final playoff games). Fixed a bug in the repetition handling, and optimized check/stalemate detection.
* <b>V1.19</b> Fixed potential out of bounds exception. Fixed bug in stalemate detection.
* <b>V1.20</b> Fixed (another) bug in the repetition detection.

[There will be no API changes after August 1]

## How to Participate
* Install an IDE such as [Visual Studio](https://visualstudio.microsoft.com/downloads/).
* Install [.NET 6.0](https://dotnet.microsoft.com/en-us/download)
* Download this repository and open the Chess-Challenge project in your IDE.
* Try building and running the project.
  * If a window with a chess board appears — great!
  * If it doesn't work, take a look at the [FAQ/troubleshooting](#faq-and-troubleshooting) section at the bottom of the page. You can also search the [issues page](https://github.com/SebLague/Chess-Challenge/issues) to see if anyone is having a similar issue. If not, post about it there with any details such as error messages, operating system etc.
* Open the MyBot.cs file _(located in src/MyBot)_ and write some code!
  * You might want to take a look at the [Documentation](https://seblague.github.io/chess-coding-challenge/documentation/) first, and the Rules too!
* Build and run the program again to test your changes.
  * For testing, you have three options in the program:
    * You can play against the bot yourself (Human vs Bot)
    * The bot can play a match against itself (MyBot vs MyBot)
    * The bot can play a match against a simple example bot (MyBot vs EvilBot).<br>You could also replace the EvilBot code with your own code, to test two different versions of your bot against one another.
* Once you're happy with your chess bot, head over to the [Submission Page](https://forms.gle/6jjj8jxNQ5Ln53ie6) to enter it into the competition.
  * You will be able to edit your entry up until the competition closes.

## Rules
* You may participate alone, or in a group of any size.
* You may submit a maximum of two entries.
  * Please only submit a second entry if it is significantly different from your first bot (not just a minor tweak).
  * Note: you will need to log in with a second Google account if you want submit a second entry.
* Only the following namespaces are allowed:
    * `ChessChallenge.API`
    * `System`
    * `System.Numerics`
    * `System.Collections.Generic`
    * `System.Linq`
      * You may not use the `AsParallel()` function
* As implied by the allowed namespaces, you may not read data from a file or access the internet, nor may you create any new threads or tasks to run code in parallel/in the background.
* You may not use the unsafe keyword.
* You may not store data inside the name of a variable/function/class etc (to be extracted with `nameof()`, `GetType().ToString()`, `Environment.StackTrace` and so on). Thank you to [#12](https://github.com/SebLague/Chess-Challenge/issues/12) and [#24](https://github.com/SebLague/Chess-Challenge/issues/24).
* If your bot makes an illegal move or runs out of time, it will lose the game.
   * Games are played with 1 minute per side by default (this can be changed in the settings class). The final tournament time control is TBD, so your bot should not assume a particular time control, and instead respect the amount of time left on the timer (given in the Think function).
* Your bot may not use more than 256mb of memory for creating look-up tables (such as a transposition table).
* If you have added a constructor to MyBot (for generating look up tables, etc.) it may not take longer than 5 seconds to complete.
* All of your code/data must be contained within the _MyBot.cs_ file.
   * Note: you may create additional scripts for testing/training your bot, but only the _MyBot.cs_ file will be submitted, so it must be able to run without them.
   * You may not rename the _MyBot_ struct or _Think_ function contained in the _MyBot.cs_ file.
   * The code in MyBot.cs may not exceed the _bot brain capacity_ of 1024 (see below).

## Bot Brain Capacity
There is a size limit on the code you create called the _bot brain capacity_. This is measured in ‘tokens’ and may not exceed 1024. The number of tokens you have used so far is displayed on the bottom of the screen when running the program.

All names (variables, functions, etc.) are counted as a single token, regardless of length. This means that both lines of code: `bool a = true;` and `bool myObscenelyLongVariableName = true;` count the same. Additionally, the following things do not count towards the limit: white space, new lines, comments, access modifiers, commas, and semicolons.

## FAQ and Troubleshooting
* What is the format of the tournament?
  * The format may change depending on the number of entries, but the current plan is to run two tournaments, with the first being a large Swiss tournament in which all bots are able to receive a ranking. These games will be played from the standard starting position. Some percengtage of the top bots will then be promoted to a second knock-out tournament, which will use a selection of different opening positions. The exact number of rounds/games and time-control are TBD.
* [Unable to build/run the project from my IDE/Code editor](https://github.com/SebLague/Chess-Challenge/issues/85)
  * After downloading the project and installing .Net 6.0, open a terminal / command prompt window.
  * Navigate to the folder where Chess-Challenge.csproj is located using the `cd` command.
    * For example: `cd C:\Users\MyName\Desktop\Chess-Challenge\Chess-Challenge`
  * Now use the command: `dotnet run`
  * This should launch the project. If not, open an issue with any error messages and relevant info.
*  [Running on Linux](https://github.com/SebLague/Chess-Challenge/discussions/3)
* Issues with illegal moves or errors when making/undoing a move
  * Make sure that you are making and undoing moves in the correct order, and that you don't forget to undo a move when exiting early from a function for example.
* How to tell what colour MyBot is playing
  * You can look at `board.IsWhiteToMove` when the Think function is called
* `GetPiece()` function is giving a null piece after making a move
  * Please make sure you are using the latest version of the project, there was a bug with this function in the original version
* There is a community-run discord server [over here](https://github.com/SebLague/Chess-Challenge/discussions/156).
* There is also an unofficial [live leaderboard](https://chess.stjo.dev/) created by a member of the community (source code available [here](https://github.com/StanislavNikolov/chess-league)).
  
