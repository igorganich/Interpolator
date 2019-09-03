using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;

using LeastSquareMethod;

namespace LeastSquareMethod
{
    public class Matrix
    {
        public double[,] Args { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }

        public Matrix(double[] x)
        {
            Row = x.Length;
            Col = 1;
            Args = new double[Row, Col];
            for (int i = 0; i < Args.GetLength(0); i++)
                for (int j = 0; j < Args.GetLength(1); j++)
                    Args[i, j] = x[i];
        }

        public Matrix(double[,] x)
        {
            Row = x.GetLength(0);
            Col = x.GetLength(1);
            Args = new double[Row, Col];
            for (int i = 0; i < Args.GetLength(0); i++)
                for (int j = 0; j < Args.GetLength(1); j++)
                    Args[i, j] = x[i, j];
        }

        public Matrix(Matrix other)
        {
            this.Row = other.Row;
            this.Col = other.Col;
            Args = new double[Row, Col];
            for (int i = 0; i < Row; i++)
                for (int j = 0; j < Col; j++)
                    this.Args[i, j] = other.Args[i, j];
        }

        public override string ToString()
        {
            string s = string.Empty;
            for (int i = 0; i < Args.GetLength(0); i++)
            {
                for (int j = 0; j < Args.GetLength(1); j++)
                {
                    s += string.Format("{0} ", Args[i, j]);
                }
                s += "\n";
            }
            return s;
        }

        public Matrix Transposition()
        {
            double[,] t = new double[Col, Row];
            for (int i = 0; i < Row; i++)
                for (int j = 0; j < Col; j++)
                    t[j, i] = Args[i, j];
            return new Matrix(t);
        }

        public static Matrix operator *(Matrix m, double k)
        {
            Matrix ans = new Matrix(m);
            for (int i = 0; i < ans.Row; i++)
                for (int j = 0; j < ans.Col; j++)
                    ans.Args[i, j] = m.Args[i, j] * k;
            return ans;
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.Col != m2.Row) throw new ArgumentException("Multiplication of these two matrices can't be done!");
            double[,] ans = new double[m1.Row, m2.Col];
            for (int i = 0; i < m1.Row; i++)
            {
                for (int j = 0; j < m2.Col; j++)
                {
                    for (int k = 0; k < m2.Row; k++)
                    {
                        ans[i, j] += m1.Args[i, k] * m2.Args[k, j];
                    }
                }
            }
            return new Matrix(ans);
        }

        private Matrix getMinor(int row, int column)
        {
            if (Row != Col) throw new ArgumentException("Matrix should be square!");
            double[,] minor = new double[Row - 1, Col - 1];
            for (int i = 0; i < this.Row; i++)
            {
                for (int j = 0; j < this.Col; j++)
                {
                    if ((i != row) || (j != column))
                    {
                        if (i > row && j < column) minor[i - 1, j] = this.Args[i, j];
                        if (i < row && j > column) minor[i, j - 1] = this.Args[i, j];
                        if (i > row && j > column) minor[i - 1, j - 1] = this.Args[i, j];
                        if (i < row && j < column) minor[i, j] = this.Args[i, j];
                    }
                }
            }
            return new Matrix(minor);
        }

        public static double Determ(Matrix m)
        {
            if (m.Row != m.Col) throw new ArgumentException("Matrix should be square!");
            double det = 0;
            int length = m.Row;

            if (length == 1) det = m.Args[0, 0];
            if (length == 2) det = m.Args[0, 0] * m.Args[1, 1] - m.Args[0, 1] * m.Args[1, 0];

            if (length > 2)
                for (int i = 0; i < m.Col; i++)
                    det += Math.Pow(-1, 0 + i) * m.Args[0, i] * Determ(m.getMinor(0, i));

            return det;
        }

        public Matrix MinorMatrix()
        {
            double[,] ans = new double[Row, Col];

            for (int i = 0; i < Row; i++)
                for (int j = 0; j < Col; j++)
                    ans[i, j] = Math.Pow(-1, i + j) * Determ(this.getMinor(i, j));

            return new Matrix(ans);
        }

        public Matrix InverseMatrix()
        {
            if (Math.Abs(Determ(this)) <= 0.000000001) throw new ArgumentException("Inverse matrix does not exist!");

            double k = 1 / Determ(this);

            Matrix minorMatrix = this.MinorMatrix();

            return minorMatrix * k;
        }
    }
}

namespace LeastSquareMethod
{
    // ВНИМАНИЕ! Необходимо еще в некоторых местах сделать проверки на null
    public class LSM
    {
        // Массивы значений Х и У задаются как свойства
        public double[] X { get; set; }
        public double[] Y { get; set; }

        // Искомые коэффициенты полинома в данном случае, а в общем коэфф. при функциях
        private double[] coeff;
        public double[] Coeff { get { return coeff; } }

        // Среднеквадратичное отклонение
        public double? Delta { get { return getDelta(); } }

        // Конструктор класса. Примает 2 массива значений х и у
        // Длина массивов должна быть одинакова, иначе нужно обработать исключение
        public LSM(double[] x, double[] y)
        {
            if (x.Length != y.Length) throw new ArgumentException("X and Y arrays should be equal!");
            X = new double[x.Length];
            Y = new double[y.Length];

            for (int i = 0; i < x.Length; i++)
            {
                X[i] = x[i];
                Y[i] = y[i];
            }
        }

        // Собственно, Метод Наименьших Квадратов
        // В качестве базисных функций используются степенные функции y = a0 * x^0 + a1 * x^1 + ... + am * x^m
        public void Polynomial(int m)
        {
            if (m <= 0) throw new ArgumentException("Порядок полинома должен быть больше 0");
            if (m >= X.Length) throw new ArgumentException("Порядок полинома должен быть на много меньше количества точек!");

            // массив для хранения значений базисных функций
            double[,] basic = new double[X.Length, m + 1];

            // заполнение массива для базисных функций
            for (int i = 0; i < basic.GetLength(0); i++)
                for (int j = 0; j < basic.GetLength(1); j++)
                    basic[i, j] = Math.Pow(X[i], j);

            // Создание матрицы из массива значений базисных функций(МЗБФ)
            Matrix basicFuncMatr = new Matrix(basic);

            // Транспонирование МЗБФ
            Matrix transBasicFuncMatr = basicFuncMatr.Transposition();

            // Произведение транспонированного  МЗБФ на МЗБФ
            Matrix lambda = transBasicFuncMatr * basicFuncMatr;

            // Произведение транспонированого МЗБФ на следящую матрицу 
            Matrix beta = transBasicFuncMatr * new Matrix(Y);

            // Решение СЛАУ путем умножения обратной матрицы лямбда на бету
            Matrix a = lambda.InverseMatrix() * beta;

            // Присвоение значения полю класса 
            coeff = new double[a.Row];
            for (int i = 0; i < coeff.Length; i++)
            {
                coeff[i] = a.Args[i, 0];
            }
        }

        // Функция нахождения среднеквадратичного отклонения
        private double? getDelta()
        {
            if (coeff == null) return null;
            double[] dif = new double[Y.Length];
            double[] f = new double[X.Length];
            for (int i = 0; i < X.Length; i++)
            {
                for (int j = 0; j < coeff.Length; j++)
                {
                    f[i] += coeff[j] * Math.Pow(X[i], j);
                }
                dif[i] = Math.Pow((f[i] - Y[i]), 2);
            }
            return Math.Sqrt(dif.Sum() / X.Length);
        }
    }
}

public class buttonscript : MonoBehaviour
{
    GameObject[] circles = new GameObject[5];
    GameObject[] square_array = new GameObject[61];
    GameObject[] lagrange_array = new GameObject[61];
    GameObject[] renders = new GameObject[2];

    bool is_printed = false;
    public GameObject circle;
    public Transform pos;
    public GameObject squares;
    public GameObject lanranges;
    public GameObject renderer;

    Vector3 smth; 
    // Start is called before the first frame update
    void Start()
    {
        smth = new Vector3(-6.1f, -2.5f, -4f);
        for (int i = 0; i < 5; i++)
        {
            circles[i] = Instantiate(circle, smth, pos.rotation); // spawn circles
            smth[0] += (float)(6.1 - 3.4);
        }
    }

    void print_graph_squares(double[] args)
    {
        int count = 0;
        renders[0].GetComponent<LineRenderer>().SetVertexCount(61);
        for (double x = 0; x < 6; x += 0.1)
        {
            double y = args[0] + args[1] * x + args[2] * System.Math.Pow(x, 2) + args[3] * System.Math.Pow(x, 3);
            smth[0] = (float)(-8.8 + x * ((8.8 - 6.15)));
            smth[1] = (float)(-3.9 + y * 1.4);
            square_array[count] = Instantiate(squares, smth, pos.rotation);
            renders[0].GetComponent<LineRenderer>().SetPosition(count, square_array[count].transform.position);
            count++;
        }
    }

    public Color32 ToColor(int HexVal)
    {
        byte R = (byte)((HexVal >> 16) & 0xFF);
        byte G = (byte)((HexVal >> 8) & 0xFF);
        byte B = (byte)((HexVal) & 0xFF);
        return new Color32(R, G, B, 255);
    }

    void squares_method(double[] x, double[] y)
    {
        LSM myReg = new LSM(x, y);
        double[] args = new double[4];
        // Апроксимация заданных значений линейным полиномом
        myReg.Polynomial(3);
        Debug.Log(myReg.Coeff.Length + " is len");
        for (int i = 0; i < myReg.Coeff.Length; i++)
        {
            args[i] = myReg.Coeff[i];
        }
        renders[0] = Instantiate(renderer);
        Color c1 = new Color(255, 2, 0, 255);
        LineRenderer lineRenderer = renders[0].GetComponent<LineRenderer>();
        lineRenderer.material.color = c1;
        lineRenderer.material.SetFloat("_Metallic", 0.94f);
        lineRenderer.SetColors(c1, c1);
        print_graph_squares(args);
    }

    static double InterpolateLagrangePolynomial(double x, double[] xValues, double[] yValues, int size)
    {
        double lagrangePol = 0;

        for (int i = 0; i < size; i++)
        {
            double basicsPol = 1;
            for (int j = 0; j < size; j++)
            {
                if (j != i)
                {
                    basicsPol *= (x - xValues[j]) / (xValues[i] - xValues[j]);
                }
            }
            lagrangePol += basicsPol * yValues[i];
        }

        return lagrangePol;
    }

    void lagrange_method(double[] x, double[] y)
    {
        renders[1] = Instantiate(renderer);
        Color c1 = new Color(246, 255, 0, 255);
        LineRenderer lineRenderer = renders[1].GetComponent<LineRenderer>();
        Debug.Log(lineRenderer.material.color);
        lineRenderer.material.color = c1;
        lineRenderer.material.SetFloat("_Metallic", 0.94f);
        lineRenderer.SetColors(c1, c1);
        lineRenderer.SetVertexCount(61);
        int count = 0;
        for (double i = 0; i < 6; i += 0.1)
        {
            double ypos = InterpolateLagrangePolynomial(i, x, y, 5);
            smth[0] = (float)(-8.8 + i * ((8.8 - 6.15)));
            smth[1] = (float)(-3.9 + ypos * 1.4);
            lagrange_array[count] = Instantiate(lanranges, smth, pos.rotation);
            lineRenderer.SetPosition(count, lagrange_array[count].transform.position);
            count++;
        }
    }

    void OnMouseDown()
    {
        if (is_printed)
        {
            for (int i = 0; i < 61; i++)
            {
                Destroy(square_array[i]);
                Destroy(lagrange_array[i]);
            }
            Destroy(renders[0]);
            Destroy(renders[1]);
        }
        is_printed = true;
        double[] x = new double[] { 1, 2, 3, 4, 5 };
        double[] y = new double[5];
        for (int i = 1; i < 6; i++)
        {
            y[i - 1] = (circles[i - 1].transform.position[1] - (-3.9)) / 1.4;
        }
        squares_method(x, y);
        lagrange_method(x, y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
