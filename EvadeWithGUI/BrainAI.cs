using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EvadeWithGUI
{

    public class BrainAI
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public GameRules Rules { get; set; }

        public BrainAI()
        {
            Min = GameConstants.min;
            Max = GameConstants.max;
            Rules = new GameRules();
        }
        /*
        public List<int> RandomMove(GameBoard board, int playerColor)
        {
            Tuple<int, int> tuple = board.SelectRandomPiece(playerColor);
            List<List<int>> moves = rules.PosibleMoves(tuple.Item1, tuple.Item2, board);

            var random = new Random();
            int randomIndex = random.Next(moves.Count);
            List<int> result = moves[randomIndex];
            return result;
        }
        */

        public bool MaximizingPlayer(int playerColor)
        {
            if (playerColor == (int)GameConstants.PlayerColor.Black)
                return true;
            else
                return false;
        }

        public List<int> SmartMove(GameBoard board, int playerColor, int IQ, CancellationToken cancellationToken)
        {
            int depth = IQ;
            GameBoard boardCopy = board;

            List<List<int>> allPlayerMoves = AllPlayerMoves(board, playerColor);

            int bestEval;

            if (MaximizingPlayer(playerColor))
                bestEval = Min;
            else
                bestEval = Max;

            Shuffle(allPlayerMoves);

            List<int> bestMove = allPlayerMoves[0];

            foreach (List<int> move in allPlayerMoves)
            {

                boardCopy.MakeMove(move, boardCopy);
                int eval = Minimax(boardCopy, depth, Min, Max, MaximizingPlayer(playerColor), cancellationToken);

                if (MaximizingPlayer(playerColor) && eval > bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }
                if (!MaximizingPlayer(playerColor) && eval < bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }

                UndoMove(board, move);
            }

            //Console.WriteLine();
            //Console.WriteLine("Nejlepší ohodnocení: " + bestEval);

            return bestMove;

        }

        public int Minimax(GameBoard board, int depth, int alpha, int beta, bool maximizingPlayer, CancellationToken cancellationToken)
        {
            int eval;
            int maxEval;
            int minEval;

            if (Rules.EndGame(board) || depth == 0)
            {
                return HeuristicEvaluation(board, maximizingPlayer);
            }
            else if (maximizingPlayer)
            {
                List<List<int>> allPlayerMoves = AllPlayerMoves(board, (int)GameConstants.PlayerColor.Black);
                maxEval = Min;

                foreach (List<int> move in allPlayerMoves)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return 0;
                    }

                    board.MakeMove(move, board);
                    eval = Minimax(board, depth - 1, alpha, beta, false, cancellationToken);
                    board = UndoMove(board, move);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break;
                }
                return maxEval;
            }
            else
            {
                minEval = Max;
                List<List<int>> allPlayerMoves = AllPlayerMoves(board, (int)GameConstants.PlayerColor.White);

                foreach (List<int> move in allPlayerMoves)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return 0;
                    } 

                    board.MakeMove(move, board);
                    eval = Minimax(board, depth - 1, alpha, beta, true, cancellationToken);
                    board = UndoMove(board, move);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }

        }

        public int HeuristicEvaluation(GameBoard board, bool maximizingPlayer)
        {
            int playerColor;
            if (maximizingPlayer)
                playerColor = (int)GameConstants.PlayerColor.Black;
            else
                playerColor = (int)GameConstants.PlayerColor.White;

            if (Rules.EGRoyalLine(board))
            {
                if (playerColor == 1)
                    return Max;
                else
                    return Min;
            }
            else
            {
                int boardEval = BoardScoreSum(board, playerColor);
                return (boardEval);
            }
        }

        public int Swap(int playerColor)
        {
            if (playerColor == (int)GameConstants.PlayerColor.White)
                return (int)GameConstants.PlayerColor.Black;
            else if (playerColor == (int)GameConstants.PlayerColor.Black)
                return (int)GameConstants.PlayerColor.White;
            else return 0;
        }

        public int BoardScoreSum(GameBoard gameBoard, int playerColor)
        {
            int[,] board = gameBoard.Board;

            int sum = 0;
            for (int i = 1; i < board.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < board.GetLength(1) - 1; j++)
                {
                    if (!gameBoard.Frozen(i, j))
                        sum += board[i, j];

                    if (gameBoard.IsWhiteKing(i, j))
                    {
                        sum -= 15;

                        sum += CheckKingSiege(gameBoard, i, j);
                        sum -= CheckKingMarch(i, playerColor);
                    }
                    if (gameBoard.IsWhite(i, j) && !gameBoard.IsWhiteKing(i, j))
                    {
                        sum -= 10;
                        sum -= KingHunting(gameBoard, i, j);
                    }

                    if (gameBoard.IsBlackKing(i, j))
                    {
                        sum += 15;
                        sum -= CheckKingSiege(gameBoard, i, j);
                        sum += CheckKingMarch(i, playerColor);
                    }
                    if (gameBoard.IsBlack(i, j) && !gameBoard.IsBlackKing(i, j))
                    {
                        sum += 10;
                        sum += KingHunting(gameBoard, i, j);
                    }

                }
            }
            return sum;
        }

        public int KingHunting(GameBoard board, int row, int col)
        {
            int hunt = 0;
            List<List<int>> adjacted = GameRules.Adjacted(row, col, board);

            foreach (List<int> position in adjacted)
            {
                if (board.Enemy(row, col, position[0], position[1]) && board.IsKing(position[0], position[1]))
                    hunt += 2;
            }
            return hunt;
        }

        public int CheckKingSiege(GameBoard board, int row, int col)
        {
            int sieged = 0;
            List<List<int>> adjacted = GameRules.Adjacted(row, col, board);

            foreach (List<int> position in adjacted)
            {
                if (board.Enemy(row, col, position[0], position[1]) && !board.IsKing(position[0], position[1]))
                    sieged = 20;
            }
            return sieged;
        }

        /*
        public bool KingsDead(GameBoard gameBoard, bool maximizingPlayer)
        {
            int[,] board = gameBoard.board;
            int playerColor;
            if (maximizingPlayer)
                playerColor = (int)GameConstants.PlayerColor.Black;
            else
                playerColor = (int)GameConstants.PlayerColor.White;

            for (int i = 1; i < board.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < board.GetLength(1) - 1; j++)
                {
                    if (gameBoard.IsBlackKing(i, j) && playerColor == (int)GameConstants.PlayerColor.Black)
                        return false;
                    if (gameBoard.IsWhiteKing(i, j) && playerColor == (int)GameConstants.PlayerColor.White)
                        return false;
                }
            }
            return true;
        }
        */

        public int CheckKingMarch(int row, int playerColor)
        {
            int march = 7;

            if (playerColor == (int)GameConstants.PlayerColor.White)
            {
                return row - 7;
            }
            if (playerColor == (int)GameConstants.PlayerColor.Black)
            {
                return (march - row) - 7;
            }
            else
                return 0;

        }

        public GameBoard UndoMove(GameBoard board, List<int> move)
        {
            board.Board[move[0], move[1]] = move[2];
            board.Board[move[3], move[4]] = move[5];
            return board;
        }

        void Shuffle<T>(List<T> list)
        {
            System.Random random = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                int k = random.Next(n);
                n--;
                T temp = list[k];
                list[k] = list[n];
                list[n] = temp;
            }
        }

        public List<List<int>> AllPlayerMoves(GameBoard board, int playerColor)
        {
            List<List<int>> allPossibleMoves = new List<List<int>>(100);
            List<Tuple<int, int>> allPieces = board.AllPlayerPieces(playerColor);

            foreach (Tuple<int, int> piece in allPieces)
            {
                int row = piece.Item1;
                int col = piece.Item2;
                List<List<int>> piecePossibleMoves = Rules.PosibleMoves(piece.Item1, piece.Item2, board);
                foreach (List<int> move in piecePossibleMoves)
                {
                    allPossibleMoves.Add(move);
                }
            }

            return allPossibleMoves;
        }

    }
}
