using System;
using System.Collections.Generic;

// Used a lot of code from https://www.geeksforgeeks.org/minimax-algorithm-in-game-theory-set-3-tic-tac-toe-ai-finding-optimal-move/
namespace tts_server
{
    class Game
    {
        public class Move
        {
            public int row, col;
        };

        public readonly static int player = 0, opponent = 1;

        private int[,] _board = pristineBoard();

        public int wins,losses,ties = 0;

        public int[,] Board
        {
            get { return _board; }
        }

        public bool Finished
        { get; set; }

        private static bool RandomStumble(double chance = 0.5)
        {
            double rd = new Random().NextDouble();
            bool stumble = rd <= chance;
            Console.WriteLine("The AI {0} make a stumble move.", (stumble ? "did" : "did not"));
            return stumble;
        }

        public void placeTic(int who, int row, int col)
        {
            if(isLegalMove(row, col))
                _board[row, col] = who;
            else
                throw new InvalidMoveException(row, col);
        }

        public void resetGame()
        {
            Finished = false;
            _board = pristineBoard();
        }

        static public int[,] pristineBoard()
        {
            int[,] board = {{ -1, -1, -1 },
                            { -1, -1, -1 },
                            { -1, -1, -1 }};
            return board;
        }

        // This function returns true if there are moves
        // remaining on the board. It returns false if
        // there are no moves left to play.
        public bool isMovesLeft()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (_board[i, j] == -1)
                        return true;
            return false;
        }

        public bool isLegalMove(int row, int col)
        {
            return _board[row, col] == -1;
        }

        // This is the evaluation function as discussed
        // in the previous article ( http://goo.gl/sJgv68 )
        private int evaluate(bool randomStumble)
        {
            // Checking for Rows for X or O victory.
            for (int row = 0; row < 3; row++)
            {
                if (_board[row, 0] == _board[row, 1] &&
                    _board[row, 1] == _board[row, 2])
                {
                    if (_board[row, 0] == opponent)
                        return +10;
                    else if (_board[row, 0] == player)
                        return -10;
                }
            }

            // Checking for Columns for X or O victory.
            for (int col = 0; col < 3; col++)
            {
                if (_board[0, col] == _board[1, col] &&
                    _board[1, col] == _board[2, col])
                {
                    if (_board[0, col] == opponent)
                        return +10;

                    else if (_board[0, col] == player)
                        return -10;
                }
            }

            //I found out, after a large amount of inefficient testing, that decreasing the player values (in this case, horizontal wins and left diagonals)

            // Checking for Diagonals for X or O victory.
            if (_board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2])
            {
                if (_board[0, 0] == opponent)
                    return +10;
                else if (_board[0, 0] == player)
                    return -10;
            }

            if (_board[0, 2] == _board[1, 1] && _board[1, 1] == _board[2, 0])
            {
                if (_board[0, 2] == opponent)
                    return +10;
                else if (_board[0, 2] == player)
                    return -10;
            }

            // Else if none of them have won then return 0
            return 0;
        }

        // This is the minimax function. It considers all
        // the possible ways the game can go and returns
        // the value of the board
        private int minimax(int depth, bool isMax, bool randomStumble = false)
        {
            int score = evaluate(randomStumble);

            // If Maximizer has won the game
            // return his/her evaluated score
            if (score == 10)
                return score;

            // If Minimizer has won the game
            // return his/her evaluated score
            if (score == -10)
                return score;

            // If there are no more moves and
            // no winner then it is a tie
            if (isMovesLeft() == false)
                return 0;

            // If this maximizer's move
            if (isMax)
            {
                int best = -1000;

                // Traverse all cells
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        // Check if cell is empty
                        if (_board[i, j] == -1)
                        {
                            // Make the move
                            _board[i, j] = opponent;

                            // Call minimax recursively and choose
                            // the maximum value
                            best = Math.Max(best, minimax(depth + 1, !isMax));

                            // Undo the move
                            _board[i, j] = -1;
                        }
                    }
                }
                return best;
            }

            // If this minimizer's move
            else
            {
                int best = 1000;

                // Traverse all cells
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        // Check if cell is empty
                        if (_board[i, j] == -1)
                        {
                            // Make the move
                            _board[i, j] = player;

                            // Call minimax recursively and choose
                            // the minimum value
                            best = Math.Min(best, minimax(depth + 1, !isMax));

                            // Undo the move
                            _board[i, j] = -1;
                        }
                    }
                }
                return best;
            }
        }

        // This will return the best possible
        // move for the player
        public Move findBestMove()
        {
            int bestVal = -1000;
            Move bestMove = new Move();
            bestMove.row = -1;
            bestMove.col = -1;

            bool randomStumble = RandomStumble(.25);

            // Select a random cell if AI stumbles
            if (randomStumble)
                return RandomMove();

            // Traverse all cells, evaluate minimax function
            // for all empty cells. And return the cell
            // with optimal value.
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    // Check if cell is empty
                    if (_board[i, j] == -1)
                    {
                        // Make the move
                        _board[i, j] = opponent;

                        // compute evaluation function for this
                        // move.
                        int moveVal = minimax(0, false, randomStumble);

                        // Undo the move
                        _board[i, j] = -1;

                        // If the value of the current move is
                        // more than the best value, then update
                        // best/
                        if (moveVal > bestVal)
                        {
                            bestMove.row = i;
                            bestMove.col = j;
                            bestVal = moveVal;
                        }
                    }
                }
            }

            return bestMove;
        }

        private Move RandomMove()
        {
            List<Move> randCells = new List<Move>();

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (_board[i, j] == -1) {
                        Move move = new Move();
                        move.row = i;
                        move.col = j;
                        randCells.Add(move);
                    }

            return randCells[new Random().Next(0, randCells.Count)];
        }

        public bool checkWinner(int player)
        {
            // check rows
            if (_board[0, 0] == player && _board[0, 1] == player && _board[0, 2] == player) { return true; }
            if (_board[1, 0] == player && _board[1, 1] == player && _board[1, 2] == player) { return true; }
            if (_board[2, 0] == player && _board[2, 1] == player && _board[2, 2] == player) { return true; }

            // check columns
            if (_board[0, 0] == player && _board[1, 0] == player && _board[2, 0] == player) { return true; }
            if (_board[0, 1] == player && _board[1, 1] == player && _board[2, 1] == player) { return true; }
            if (_board[0, 2] == player && _board[1, 2] == player && _board[2, 2] == player) { return true; }

            // check diags
            if (_board[0, 0] == player && _board[1, 1] == player && _board[2, 2] == player) { return true; }
            if (_board[0, 2] == player && _board[1, 1] == player && _board[2, 0] == player) { return true; }

            return false;
        }
    }

    [Serializable]
    class InvalidMoveException : Exception
    {
        public InvalidMoveException() { }

        public InvalidMoveException(int row, int col)
            : base(String.Format("Invalid move : [{0},{1}]", row, col))
        {
        }
    }

    [Serializable]
    class GameFinishedException : Exception
    {
        public GameFinishedException() { }

        public GameFinishedException(int row, int col)
            : base(String.Format("Game is already finished"))
        {
        }
    }
}
