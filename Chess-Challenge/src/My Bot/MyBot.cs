using ChessChallenge.API;

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

    public Move Think(Board board, Timer timer)
    {
        this.board = board;
        bestScore = -999999;

        AlphaBetaSearch(startDepth, -999999, 999999);

        return bestMove;
    }

    private int AlphaBetaSearch(int depth, int alpha, int beta)
    {
        if (depth == 0) 
        {
            return QuiescenceSearch(alpha, beta);
        }
        
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
        int scoreCp = 0;
        
        var AddMaterialScoreForColor = (bool whiteColor, int multiplier) =>
        {
            var AddMaterialScoreForPiece = (PieceType pieceType, int score) => 
                scoreCp += board.GetPieceList(pieceType, whiteColor).Count * score * multiplier;
            AddMaterialScoreForPiece(PAWN, 100);
            AddMaterialScoreForPiece(KNIGHT, 300);
            AddMaterialScoreForPiece(BISHOP, 350);
            AddMaterialScoreForPiece(ROOK, 500);
            AddMaterialScoreForPiece(QUEEN, 1000);
        };

        var AddPositionalScoreForColor = (bool whiteColor, int multiplier) =>
        {
            
        };

        int whiteMultiplier = board.IsWhiteToMove ? 1 : -1;
        AddMaterialScoreForColor(true, whiteMultiplier);
        AddPositionalScoreForColor(true, whiteMultiplier);
        AddMaterialScoreForColor(false, -whiteMultiplier);
        AddPositionalScoreForColor(false, -whiteMultiplier);
        return scoreCp;
    }
}
