using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWithGUI
{
    public class GameRules
    {
        // Metody pro ověření konce hry (EG)
        public bool EGFrozenKings(GameBoard gameBoard)
    {
        for (int row = 0; row < gameBoard.GetLength(1); row++)
        {
            for (int col = 0; col < gameBoard.GetLength(0); col++)
            {
                if (gameBoard.IsKing(row, col))
                    return false;
            }
        }
        return true;
    }

    public bool EGRoyalLine(GameBoard gameBoard)
    {
        for (int col = 0; col < gameBoard.GetLength(0); col++)
        {
            if (gameBoard.IsBlackKing(1, col))
                return true;
        }

        for (int col = 0; col < gameBoard.GetLength(0); col++)
        {
            if (gameBoard.IsWhiteKing(6, col))
                return true;
        }

        return false;
    }

    public bool EGEnemyBlocked(GameBoard gameBoard, Player playerOnTurn)
    {
        for (int row = 0; row < gameBoard.GetLength(1); row++)
        {
            for (int col = 0; col < gameBoard.GetLength(0); col++)
            {
                if (!gameBoard.IsEmpty(row, col))
                {
                    if (playerOnTurn.PlayerColor == (int)GameConstants.PlayerColor.Black)
                    {
                        if (gameBoard.IsWhite(row, col) && PosibleMoves(row, col, gameBoard).Any())
                            //if (board.PosibleMoves(row, col).Any())
                            return false;
                        continue;
                    }
                    if (playerOnTurn.PlayerColor == (int)GameConstants.PlayerColor.White)
                    {
                        if (gameBoard.IsBlack(row, col) && PosibleMoves(row, col, gameBoard).Any())
                            //if (board.PosibleMoves(row, col).Any())
                            return false;
                        continue;
                    }

                }
            }
        }
        return true;
    }

    public bool EndGame(GameBoard gameBoard)
    {
        if (EGFrozenKings(gameBoard))
        {
            return true;
        }
        if (EGRoyalLine(gameBoard))
        {
            return true;
        }
        else return false;

    }

    // Metoda pro inicializaci startovních pozic v 2D poli s pomocí pomocných metod
    public void InitBoardByRules(int[,] board)
    {
        for (int row = 0; row < board.GetLength(1); row++)
        {
            for (int col = 0; col < board.GetLength(0); col++)
            {
                if (Barrier(row, col))
                    board[row, col] = (int)GameConstants.States.barrier;
                else if (WStartPosition(row))
                {
                    if (KingStartPosition(col))
                        board[row, col] = (int)GameConstants.States.wKing;
                    else
                        board[row, col] = (int)GameConstants.States.wMan;
                }
                else if (BStartPosition(row))
                {

                    if (KingStartPosition(col))
                        board[row, col] = (int)GameConstants.States.bKing;
                    else
                        board[row, col] = (int)GameConstants.States.bMan;
                }
                else
                    board[row, col] = (int)GameConstants.States.empty;

            }
        }
    }

    // Pomocné metody pro inicializaci hrací desky
    private bool Barrier(int row, int col)
    {
        if (row == 0 || col == 0 || row == GameConstants.boardSize - 1 || col == GameConstants.boardSize - 1)
            return true;
        else
            return false;
    }

    private bool WStartPosition(int row)
    {
        if (row == 1)
            return true;
        else
            return false;
    }

    private bool BStartPosition(int row)
    {
        if (row == 6)
            return true;
        else
            return false;
    }

    private bool KingStartPosition(int col)
    {
        if (col == 3 || col == 4)
            return true;
        else
            return false;
    }

    // Metoda pro rozhodnutí zda-li je tah v souladu s pravidly
    public bool ValidMove(int row, int col, int newRow, int newCol, GameBoard board)
    {
        if (ValidPosition(row, col) && ValidPosition(newRow, newCol))
        {
            //Console.WriteLine("Valid position, Valid new position");
            if (ValidDistance(row, col, newRow, newCol))
            {
                if (board.Occupied(row, col) && !board.Frozen(row, col) && !board.Frozen(newRow, newCol))
                {
                    if (board.IsKing(row, col) && board.Occupied(newRow, newCol))
                    {
                        return false;
                    }
                    else if (board.Occupied(newRow, newCol) && !board.Enemy(row, col, newRow, newCol))
                        return false;
                    else
                        return true;
                }
                else
                    return false; //Console.WriteLine("Empty or frozen field.");  
            }
            else
                return false;
        }
        else
            return false; //Console.WriteLine("Not valid position.");
    }

    // Pomocné metody pro validaci tahu

    private bool ValidPosition(int row, int col)
    {
        if (row <= GameConstants.boardSize && col <= GameConstants.boardSize && !Barrier(row, col))
            return true;
        else
            return false;
    }

    private bool ValidDistance(int row, int col, int newRow, int newCol)
    {
        if (Math.Abs(row - newRow) <= 1 && Math.Abs(col - newCol) <= 1)
        {
            if (!(Math.Abs(row - newRow) == 0 && Math.Abs(col - newCol) == 0))
            {
                //Console.WriteLine("Valid distance.");
                return true;
            }
            else
            {
                //Console.WriteLine("NOT Valid distance.");
                return false;
            }
        }
        else
        {
            //Console.WriteLine("NOT Valid distance.");
            return false;
        }
    }

    // Metoda pro ohodnocení možného tahu
    public void EvaluateMove(int row, int col, int newRow, int newCol, GameBoard board, out int moveResult)
    {
        if (board.Position(newRow, newCol) == (int)GameConstants.States.empty)
        {
            /*
            Console.WriteLine("Moved");
            board[newRow, newCol] = board[row, col];
            Empty(row, col);
            */
            board.Move(row, col, newRow, newCol);
            moveResult = (int)GameConstants.MoveResult.Moved;
        }
        else if (board.Occupied(newRow, newCol) && board.Enemy(row, col, newRow, newCol))
        {
            /*
            Console.WriteLine("Freezed");
            Freeze(newRow, newCol);
            Empty(row, col);
            */
            board.Freeze(row, col, newRow, newCol);
            moveResult = (int)GameConstants.MoveResult.Frozen;
        }
        else
            moveResult = (int)GameConstants.MoveResult.Fail;
    }

    // Metody pro zjištění všech možných tahů
    public static List<List<int>> Adjacted(int row, int col, GameBoard board)
    {
        var listAdjacted = new List<List<int>>(8);

        for (int dx = -1; dx <= 1; ++dx)
        {
            for (int dy = -1; dy <= 1; ++dy)
            {
                if (dx != 0 || dy != 0)
                {
                    int newRow = row + dx;
                    int newCol = col + dy;
                    List<int> xy = new List<int> { row, col, board.Position(row, col), newRow, newCol, board.Position(newRow, newCol) };
                    listAdjacted.Add(xy);
                }
            }
        }
        return listAdjacted;
    }

    public List<List<int>> PosibleMoves(int row, int col, GameBoard board)
    {
        List<List<int>> adjacted = Adjacted(row, col, board);
        List<List<int>> posibleMoves = new List<List<int>>(8);

        foreach (List<int> move in adjacted)
        {
            if (ValidMove(row, col, move[3], move[4], board))
            {
                posibleMoves.Add(move);
            }
        }
        return posibleMoves;
    }
}
}
