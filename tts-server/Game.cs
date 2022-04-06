using System;

namespace tts_server
{
    class Game
    {
        private class Move
        {
            public int row, col;
        };

        private readonly static char player = 'x', opponent = 'o';

        private char[,] _board = {{ 'x', 'o', 'x' },
                                  { '_', 'o', 'x' },
                                  { '_', '_', '_' }};

        private readonly bool solo;

        public Game(bool solo = true)
        {
            this.solo = solo;
        }

        public void placeTic(int row, int col)
        {
            if(isLegalMove(row, col))
                _board[row, col] = 'x';
            else
                throw new InvalidMoveException(row, col);
        }

      /*  public Game()
        {

            
               char[,] board = {{ 'x', 'x', 'x' },
                                 { 'o', 'o', 'x' },
                                 { '_', '_', '_' }};
     Move bestMove = findBestMove(board);

              Console.Write("The Optimal Move is :\n");
              Console.Write("ROW: {0} COL: {1}\n\n", bestMove.row, bestMove.col);
        }*/

        // This function returns true if there are moves
        // remaining on the board. It returns false if
        // there are no moves left to play.
        public bool isMovesLeft()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (_board[i, j] == '_')
                        return true;
            return false;
        }

        public bool isLegalMove(int row, int col)
        {
            return _board[row, col] == '_';
        }

        // This is the evaluation function as discussed
        // in the previous article ( http://goo.gl/sJgv68 )
        private int evaluate()
        {
            // Checking for Rows for X or O victory.
            for (int row = 0; row < 3; row++)
            {
                if (_board[row, 0] == _board[row, 1] &&
                    _board[row, 1] == _board[row, 2])
                {
                    if (_board[row, 0] == player)
                        return +10;
                    else if (_board[row, 0] == opponent)
                        return -10;
                }
            }

            // Checking for Columns for X or O victory.
            for (int col = 0; col < 3; col++)
            {
                if (_board[0, col] == _board[1, col] &&
                    _board[1, col] == _board[2, col])
                {
                    if (_board[0, col] == player)
                        return +10;

                    else if (_board[0, col] == opponent)
                        return -10;
                }
            }

            // Checking for Diagonals for X or O victory.
            if (_board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2])
            {
                if (_board[0, 0] == player)
                    return +10;
                else if (_board[0, 0] == opponent)
                    return -10;
            }

            if (_board[0, 2] == _board[1, 1] && _board[1, 1] == _board[2, 0])
            {
                if (_board[0, 2] == player)
                    return +10;
                else if (_board[0, 2] == opponent)
                    return -10;
            }

            // Else if none of them have won then return 0
            return 0;
        }

        // This is the minimax function. It considers all
        // the possible ways the game can go and returns
        // the value of the board
        private int minimax(int depth, bool isMax)
        {
            int score = evaluate();

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
                        if (_board[i, j] == '_')
                        {
                            // Make the move
                            _board[i, j] = player;

                            // Call minimax recursively and choose
                            // the maximum value
                            best = Math.Max(best, minimax(depth + 1, !isMax));

                            // Undo the move
                            _board[i, j] = '_';
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
                        if (_board[i, j] == '_')
                        {
                            // Make the move
                            _board[i, j] = opponent;

                            // Call minimax recursively and choose
                            // the minimum value
                            best = Math.Min(best, minimax(depth + 1, !isMax));

                            // Undo the move
                            _board[i, j] = '_';
                        }
                    }
                }
                return best;
            }
        }

        // This will return the best possible
        // move for the player
        Move findBestMove()
        {
            int bestVal = -1000;
            Move bestMove = new Move();
            bestMove.row = -1;
            bestMove.col = -1;

            // Traverse all cells, evaluate minimax function
            // for all empty cells. And return the cell
            // with optimal value.
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    // Check if cell is empty
                    if (_board[i, j] == '_')
                    {
                        // Make the move
                        _board[i, j] = player;

                        // compute evaluation function for this
                        // move.
                        int moveVal = minimax(0, false);

                        // Undo the move
                        _board[i, j] = '_';

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
}