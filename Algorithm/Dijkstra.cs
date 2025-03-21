using System;
using System.Collections.Generic;

namespace Assignment.Data
{
    public static class DijkstraAlgorithm
    {
        public static int[][] TwoDToJagged(int[,] twoDArray)
        {
            if (twoDArray == null)
            {
                return null; // Handle null input
            }

            int rows = twoDArray.GetLength(0);
            int cols = twoDArray.GetLength(1);

            int[][] jaggedArray = new int[rows][];

            for (int i = 0; i < rows; i++)
            {
                jaggedArray[i] = new int[cols];
                for (int j = 0; j < cols; j++)
                {
                    jaggedArray[i][j] = twoDArray[i, j];
                }
            }

            return jaggedArray;
        }
        public static double CalculateMinFuel(int n, int m, int p, int[][] matrix)
        {
            int[,] graph = new int[n * m, n * m];
            Dictionary<int, (int row, int col)> chestLocations = new Dictionary<int, (int row, int col)>();

            // Xây dựng đồ thị từ ma trận
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    chestLocations[matrix[i][j]] = (i, j);
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    int currentChest = matrix[i][j];
                    for (int nextChest = 1; nextChest <= p; nextChest++)
                    {
                        if (nextChest == currentChest) continue;

                        int nextRow = chestLocations[nextChest].row;
                        int nextCol = chestLocations[nextChest].col;

                        double fuelCost = Math.Sqrt(Math.Pow(i - nextRow, 2) + Math.Pow(j - nextCol, 2));
                        graph[i * m + j, nextRow * m + nextCol] = (int)(fuelCost * 100); // Nhân 100 để chuyển về số nguyên, tránh sai số float
                    }
                }
            }

            // Tìm đường đi ngắn nhất từ rương 0 đến rương p
            double totalFuel = 0;
            int currentRow = 0;
            int currentCol = 0;

            for (int currentChest = 1; currentChest <= p; currentChest++)
            {
                int nextRow = chestLocations[currentChest].row;
                int nextCol = chestLocations[currentChest].col;

                totalFuel += FindShortestPath(graph, currentRow * m + currentCol, nextRow * m + nextCol, n * m) / 100.0;

                currentRow = nextRow;
                currentCol = nextCol;
            }

            return totalFuel;
        }

        private static double FindShortestPath(int[,] graph, int start, int end, int verticesCount)
        {
            double[] distance = new double[verticesCount];
            bool[] shortestPathTreeSet = new bool[verticesCount];

            for (int i = 0; i < verticesCount; i++)
            {
                distance[i] = double.MaxValue;
                shortestPathTreeSet[i] = false;
            }

            distance[start] = 0;

            for (int count = 0; count < verticesCount - 1; count++)
            {
                int u = MinimumDistance(distance, shortestPathTreeSet, verticesCount);
                shortestPathTreeSet[u] = true;

                for (int v = 0; v < verticesCount; v++)
                {
                    if (!shortestPathTreeSet[v] && graph[u, v] != 0 && distance[u] != double.MaxValue && distance[u] + graph[u, v] < distance[v])
                    {
                        distance[v] = distance[u] + graph[u, v];
                    }
                }
            }

            return distance[end];
        }

        private static int MinimumDistance(double[] distance, bool[] shortestPathTreeSet, int verticesCount)
        {
            double min = double.MaxValue;
            int minIndex = -1;

            for (int v = 0; v < verticesCount; v++)
            {
                if (shortestPathTreeSet[v] == false && distance[v] <= min)
                {
                    min = distance[v];
                    minIndex = v;
                }
            }

            return minIndex;
        }
    }
}
