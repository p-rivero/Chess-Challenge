using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    private readonly PieceType PAWN = PieceType.Pawn;
    private readonly PieceType KNIGHT = PieceType.Knight;
    private readonly PieceType BISHOP = PieceType.Bishop;
    private readonly PieceType ROOK = PieceType.Rook;
    private readonly PieceType QUEEN = PieceType.Queen;
    // private readonly PieceType KING = PieceType.King;

    private Board board;
    private Move bestMove;
    private int bestScore;
    private readonly int startDepth = 4;
    private int alphabetaNodes; // #DEBUG
    private int quiescenceNodes; // #DEBUG

    public Move Think(Board board, Timer timer)
    {
        this.board = board;
        bestScore = -999999;
        alphabetaNodes = 0; // #DEBUG
        quiescenceNodes = 0; // #DEBUG

        int score = AlphaBetaSearch(startDepth, -999999, 999999);

        Console.WriteLine("{0} (score = {1})", bestMove, score); // #DEBUG
        Console.WriteLine("Nodes:        {0}", alphabetaNodes + quiescenceNodes); // #DEBUG
        Console.WriteLine("  AlphaBeta:  {0}", alphabetaNodes); // #DEBUG
        Console.WriteLine("  Quiescence: {0}", quiescenceNodes); // #DEBUG
        Console.WriteLine(); // #DEBUG

        return bestMove;
    }

    private int AlphaBetaSearch(int depth, int alpha, int beta)
    {
        if (depth == 0) 
        {
            return QuiescenceSearch(alpha, beta);
        }
        
        alphabetaNodes++; // #DEBUG
        
        if (board.IsInCheckmate())
        {
            return -100000 + startDepth - depth;
        }
        
        if (board.IsDraw())
        {
            return 0;
        }

        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            var score = -AlphaBetaSearch(depth - 1, -beta, -alpha);
            board.UndoMove(move);

            if (score > alpha)
            {
                alpha = score;

                if (score >= beta)
                {
                    return beta;
                }
            }

            if (depth == startDepth && score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        return alpha;
    }

    private int QuiescenceSearch(int alpha, int beta)
    {
        quiescenceNodes++; // #DEBUG
        int standScore = TurochampEvaluate();

        if (standScore >= beta)
        {
            return beta;
        }

        if (standScore > alpha)
        {
            alpha = standScore;
        }

        foreach (var move in board.GetLegalMoves(true))
        {
            board.MakeMove(move);
            int score = -QuiescenceSearch(-beta, -alpha);
            board.UndoMove(move);

            if (score >= beta)
            {
                return beta;
            }

            if (score > alpha)
            {
                alpha = score;
            }
        }
        return alpha;
    }


    private int TurochampEvaluate()
    {
        var AddMaterialScoreForColor = (bool whiteColor, int multiplier) =>
        {
            var AddMaterialScoreForPiece = (PieceType pieceType, int score) => board.GetPieceList(pieceType, whiteColor).Count * score * multiplier;
            return AddMaterialScoreForPiece(PAWN, 100)
                + AddMaterialScoreForPiece(KNIGHT, 300)
                + AddMaterialScoreForPiece(BISHOP, 350)
                + AddMaterialScoreForPiece(ROOK, 500)
                + AddMaterialScoreForPiece(QUEEN, 1000);
        };

        var AddPositionalScoreForCurrentPlayer = () =>
        {
            int positionalScore = 0;
            var nonPawnDefenders = NumberOfNonPawnDefenders();
            var pawnDefenders = NumberOfPawnDefenders();

			// Mobility score (rules 1, 3): use the fact that moves are grouped by piece
            int currentPieceIndex = -1;
			int currentMoveCount = 0;
            var FlushMobilityScore = () => positionalScore += (int)Math.Sqrt(10000 * currentMoveCount); // 100 * sqrt(numMoves)
            foreach (Move move in board.GetLegalMoves())
			{
                if (move.MovePieceType == PAWN || move.IsCastles)
				{
                    continue;
				}
				int fromIndex = move.StartSquare.Index;
                if (fromIndex != currentPieceIndex && currentPieceIndex != -1)
				{
                    FlushMobilityScore();
                    currentMoveCount = 0;
                }
                currentMoveCount += move.IsCapture ? 2 : 1;
                currentPieceIndex = fromIndex;
            }
            FlushMobilityScore();

            // Piece safety (rule 2)
            var AddPieceSafetyScoreNonPawn = (PieceType pieceType) =>
            {
                foreach (var piece in board.GetPieceList(pieceType, board.IsWhiteToMove))
                {
                    int index = piece.Square.Index;
                    int defenders = nonPawnDefenders[index] + pawnDefenders[index];
                    positionalScore += defenders > 1 ? 150 : defenders > 0 ? 100 : 0; // 1 point if defended, 1.5 points if defended 2+ times
                }
            };
            AddPieceSafetyScoreNonPawn(ROOK);
            AddPieceSafetyScoreNonPawn(BISHOP);
            AddPieceSafetyScoreNonPawn(KNIGHT);

            // Pawn credit (rule 6)
            foreach (var piece in board.GetPieceList(PAWN, board.IsWhiteToMove))
            {
                Square square = piece.Square;
                positionalScore += (board.IsWhiteToMove ? square.Rank - 1 : 6 - square.Rank) * 20; // 0.2 points for each rank advanced
                positionalScore += nonPawnDefenders[square.Index] > 0 ? 30 : 0; // 0.3 points if defended by a non-pawn
            }

            return positionalScore;
        };

        int whiteMultiplier = board.IsWhiteToMove ? 1 : -1;
        int scoreCp = AddMaterialScoreForColor(true, whiteMultiplier)
            + AddMaterialScoreForColor(false, -whiteMultiplier)
            + AddPositionalScoreForCurrentPlayer();
        board.ForceSkipTurn();
        scoreCp -= AddPositionalScoreForCurrentPlayer();
        board.UndoSkipTurn();
        return scoreCp;
    }

    private int[] NumberOfNonPawnDefenders()
	{
        // TODO
        return new int[64];
	}

    private int[] NumberOfPawnDefenders()
    {
        // TODO
        return new int[64];
    }
}
