using System;
using Microsoft.Spark.Sql;
using System.Diagnostics;
using System.IO;
using Sudoku.Shared;
using Sudoku.NorvigSparkSolver;



namespace Résolution.norvig
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");

            var nbWorkers = 4;
            var nbCoresPerWorker = 1;
            int nbLignesNax = 1000;

            var chronometre = Stopwatch.StartNew();

            var filePath = Path.Combine(Environment.CurrentDirectory, "sudoku.csv");

            SparkSession spark =
                 SparkSession
                      .Builder()
                      .AppName("Norvig Solver Spark")
                      .Config(" spark.executor.cores", nbCoresPerWorker)
                      .Config(" spark.executor.instances", nbWorkers)
                      .GetOrCreate();

            DataFrame dataFrame = spark
                .Read()
                .Option("header", true)
                .Schema("quizzes string, solutions string")
                .Csv(filePath);

            DataFrame milLeSudoku = dataFrame.Limit(nbLignesNax);

            spark.Udf().Register<string, string>(
                "SudokuUDF",

                (sudoku) => SolveSudoku(sudoku));
            milLeSudoku.CreateOrReplaceTempView("Resolved");
            DataFrame sqlDf = spark.Sql(" SELECT quizzes, SudokuUDF(quizzes) as Resolution from Resolved");
            sqlDf.Collect();
            sqlDf.Show();


            var tempsEcoule = chronometre.Elapsed;
            Console.WriteLine($"temps d'execution: {tempsEcoule.ToString()}");


        }


        public static string SolveSudoku(string strSudoku)
        {
            var sudoku = GridSudoku.ReadSudoku(strSudoku);
            var norvigSolver = new NorvigSolver();
            var sudokuResolu = norvigSolver.Solve(sudoku);

            //return sudokuResolu.ToString();
            string game = "";
            foreach (var ligneSudoku in sudokuResolu.Cellules)
            {
                foreach (var celluleSudoku in ligneSudoku)
                {
                    game = game + celluleSudoku.ToString();

                }
            }
            return game;
        }

    }
}
