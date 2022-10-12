using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace ContentAwareResize
{
    // *****************************************
    // DON'T CHANGE CLASS OR FUNCTION NAME
    // YOU CAN ADD FUNCTIONS IF YOU NEED TO
    // *****************************************
    public class ContentAwareResize
    {
        public struct coord
        {
            public int row;
            public int column;
        }
        private int minn(int x, int y, int z)
        {
            if (x < y && x < z)
                return x;
            else if (y < x && y < z)
                return y;
            else if (z < x && z < y)
                return z;
            else if (x == y && x == z)
                return x;
            else if (x == y && x < z)
                return x;
            else
                return z;
        }
        //========================================================================================================
        //Your Code is Here:
        //===================
        //public int[,] D;
        /// <summary>
        /// Develop an efficient algorithm to get the minimum vertical seam to be removed
        /// </summary>
        /// <param name="energyMatrix">2D matrix filled with the calculated energy for each pixel in the image</param>
        /// <param name="Width">Image's width</param>
        /// <param name="Height">Image's height</param>
        /// <returns>BY REFERENCE: The min total value (energy) of the selected seam in "minSeamValue" & List of points of the selected min vertical seam in seamPathCoord</returns>
        public void CalculateSeamsCost(int[,] energyMatrix, int Width, int Height, ref int minSeamValue, ref List<coord> seamPathCoord)
        {
            int[,] D = new int[Height+5,Width+5 ];   
            seamPathCoord = new List<coord>();
            D[0,0] = energyMatrix[0,0];
            for (int x = 1; x < Width; x++)
                D[0,x] = energyMatrix[0,x];

            for (int y = 1; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (x == 0)
                    {
                        D[y,x] = Math.Min(D[y - 1,x], D[y - 1,x + 1]) + energyMatrix[y,x];
                    }
                    else if (x == Width - 1)
                    {
                        D[y,x] = Math.Min(D[y - 1,x - 1], D[y - 1,x]) + energyMatrix[y,x];
                    }
                    else
                        D[y,x] = minn(D[y - 1,x - 1], D[y - 1,x], D[y - 1,x + 1]) + energyMatrix[y,x];
                }
            }
             minSeamValue = int.MaxValue;
            int xx = Width - 1, yy = Height - 1, zz = 0;
            while (zz <=Width-1)
            {
                if (D[yy,zz] <= minSeamValue)
                {
                    minSeamValue = D[yy,zz];
                    xx = zz;
                }
                zz++;
            }
            //cout << "YY : " << yy << "\t" << "XX : " << xx << endl;
            coord c = new coord();
            c.row = yy;
            c.column = xx;
            seamPathCoord.Add(c);
            while (yy != 0)
            {
                yy--;
                if (xx == Width - 1)
                {
                    if (D[yy,xx - 1] <= D[yy,xx])
                    {
                        xx--;
                    }
                }
                if (xx == 0)
                {
                    if (D[yy,xx + 1] < D[yy,xx])
                    {
                        xx++;
                    }
                }
                else
                {
                    if (D[yy,xx - 1] <= D[yy,xx] && D[yy,xx - 1] <= D[yy,xx + 1])
                    {
                        xx--;
                    }
                    else if (D[yy,xx] <= D[yy,xx - 1] && D[yy,xx] <= D[yy,xx + 1])
                    {
                        xx = xx;
                    }
                    else
                        xx++;
                }
                //cout << "YY : " << yy << "\t" << "XX : " << xx << endl;
                c.row = yy;
                c.column = xx;
                seamPathCoord.Add(c);
            }
        }    
        // *****************************************
        // DON'T CHANGE CLASS OR FUNCTION NAME
        // YOU CAN ADD FUNCTIONS IF YOU NEED TO 
        // *****************************************
        #region DON'TCHANGETHISCODE
        public MyColor[,] _imageMatrix;
        public int[,] _energyMatrix;
        public int[,] _verIndexMap;
        public ContentAwareResize(string ImagePath)
        {
            _imageMatrix = ImageOperations.OpenImage(ImagePath);
            _energyMatrix = ImageOperations.CalculateEnergy(_imageMatrix);
            int _height = _energyMatrix.GetLength(0);
            int _width = _energyMatrix.GetLength(1);
        }
        public void CalculateVerIndexMap(int NumberOfSeams, ref int minSeamValueFinal, ref List<coord> seamPathCoord)
        {
            int Width = _imageMatrix.GetLength(1);
            int Height = _imageMatrix.GetLength(0);

            int minSeamValue = -1;
            _verIndexMap = new int[Height, Width];
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    _verIndexMap[i, j] = int.MaxValue;

            bool[] RemovedSeams = new bool[Width]; 
            for (int j = 0; j < Width; j++)
                RemovedSeams[j] = false;

            for (int s = 1; s <= NumberOfSeams; s++)
            {
                CalculateSeamsCost(_energyMatrix, Width, Height, ref minSeamValue, ref seamPathCoord);
                minSeamValueFinal = minSeamValue;

                //Search for Min Seam # s
                int Min = minSeamValue;

                //Mark all pixels of the current min Seam in the VerIndexMap
                if (seamPathCoord.Count != Height)
                    throw new Exception("You selected WRONG SEAM");
                for (int i = Height - 1; i >= 0; i--)
                {
                    if (_verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column] != int.MaxValue)
                    {
                        string msg = "overalpped seams between seam # " + s + " and seam # " + _verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column];
                        throw new Exception(msg);
                    }
                    _verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column] = s;
                    //remove this seam from energy matrix by setting it to max value
                    _energyMatrix[seamPathCoord[i].row, seamPathCoord[i].column] = 100000;
                }

                //re-calculate Seams Cost in the next iteration again
            }
        }
        public void RemoveColumns(int NumberOfCols)
        {  
            int Width = _imageMatrix.GetLength(1);
            int Height = _imageMatrix.GetLength(0);
            _energyMatrix = ImageOperations.CalculateEnergy(_imageMatrix);

            int minSeamValue = 0;
            List<coord> seamPathCoord = null;
            //CalculateSeamsCost(_energyMatrix,Width,Height,ref minSeamValue, ref seamPathCoord);
            CalculateVerIndexMap(NumberOfCols,ref minSeamValue,ref seamPathCoord);
                
            MyColor[,] OldImage = _imageMatrix;
            _imageMatrix = new MyColor[Height, Width - NumberOfCols];
            for (int i = 0; i < Height; i++)
            {
                int cnt = 0;
                for (int j = 0; j < Width; j++)
                {
                    if (_verIndexMap[i,j] == int.MaxValue)
                        _imageMatrix[i, cnt++] = OldImage[i, j];
                }
            }
            
        }
        #endregion
    }
}
