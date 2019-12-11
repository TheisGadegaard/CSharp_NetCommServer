using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCommServer
{
    class Kalaha
    {

        private string player1, player2, activePlayer;
        private int[] board;

        public Kalaha(string player1, string player2)
        {
            this.player1 = player1;
            this.player2 = player2;
            initiateBoard();
            activePlayer = player1;
        }

        public string makeMove(int move)
        {

            Boolean extraTurn = false;
            string winner = "undecided";

            if (move > 12 || move < 0 || move == 6 ||
               (activePlayer == player1 && move > 5) ||
               (activePlayer == player2 && move < 7) ||
               board[move] == 0)
            {
                return "invalid move";
            }

            int counter = board[move];
            board[move] = 0;
            int nextPit = move;

            while (counter > 0)
            {
                nextPit++;
                if ((nextPit == 13 && activePlayer == player1) ||
                    (nextPit == 6 && activePlayer == player2))
                {
                    nextPit++;
                }
                if (nextPit == 14)
                {
                    nextPit = 0;
                }
                board[nextPit]++;
                counter--;

                if (counter == 0)
                {
                    if (board[nextPit] == 1)
                    {
                        //Rule: if place the last ball in your own pit. Add that ball and all balls directly opposing pit to your goal
                        if (activePlayer == player1 && nextPit < 6)
                        {
                            board[6] += board[nextPit] + board[12 - nextPit];
                            board[nextPit] = 0;
                            board[12 - nextPit] = 0;
                        }
                        else if (activePlayer == player2 && nextPit < 13 && nextPit > 6)
                        {
                            board[13] += board[nextPit] + board[12 - nextPit];
                            board[nextPit] = 0;
                            board[12 - nextPit] = 0;
                        }
                    }

                    if((activePlayer == player1 && nextPit == 6) || activePlayer == player2 && nextPit == 13)
                    {
                        extraTurn = true;
                    }
                }
            }

            //Rule: the game ends when all pits on one side of the field is empty
            //Rule: the player who still has pieces on his/her side of the field when the game ends capture those pieces
            if (board[0] + board[1] + board[2] + board[3] + board[4] + board[5] == 0 ||         //player1's side is empty
                board[7] + board[8] + board[9] + board[10] + board[11] + board[12] == 0)        //player2's side is empty
            {
                board[6] += board[0] + board[1] + board[2] + board[3] + board[4] + board[5];
                board[13] += board[7] + board[8] + board[9] + board[10] + board[11] + board[12];

                if (board[6] > board[13])
                {
                    winner = player1;
                }
                else if (board[6] < board[13])
                {
                    winner = player2;
                }
                else
                {
                    winner = "TIE";
                }
            }

            if (!extraTurn)
            {
                if(activePlayer == player1)
                {
                    activePlayer = player2;
                } else
                {
                    activePlayer = player1;
                }
            }

            /*
             * boolean extraTurn
             * string winner
             * int[] board
             */
            string returnValue = "KALAHA " + activePlayer + " " + winner;
            for(int i = 0; i < board.Length; i++)
            {
                returnValue += " " + board[i];
            }

            return returnValue;

          /*  return "KALAHA " + activePlayer + " " + winner + " " + board[0] + " " + board[1] + 
                                            " " + board[2] + " " + board[3] + 
                                            " " + board[4] + " " + board[5] + 
                                            " " + board[6] + " " + board[7] + 
                                            " " + board[8] + " " + board[9] + 
                                            " " + board[10] + " " + board[11] + 
                                            " " + board[12] + " " + board[13];
          */
        }


         void initiateBoard()
        {
            board = new int[14] {6, 6, 6, 6, 6, 6, 0, 6, 6, 6, 6, 6, 6, 0};
        }
    }
}
