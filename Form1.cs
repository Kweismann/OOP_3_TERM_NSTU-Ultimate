﻿using System;
using System.Drawing;
using System.Windows.Forms;

using CustomDict;
using Shapes;
using Tao.OpenGl;
using Tao.FreeGlut;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        private CustomDict<uint, double> figuresList = new CustomDict<uint, double>();//Словарь
        private SortedDictionary<double, dynamic> ShapesTree = new SortedDictionary<double, dynamic>();//Бинарное дерево .Net
        private short count_click = 0;// кол-во кликов для произвольного ввода точек
        private Point[] temp_points = new Point[6];// массив для произвольного ввода точек
        private bool flag_input = false;// флаг для произвольного ввода точек
        private Shape Gtemp = new Shape();//фигура для динамической отрисовки
        private uint pointer_shape = 1;//указатель на текущую фигуру
        public const int PARALLELOGRAM = 14, PENTAGON = 8, ELLIPSE = 6, RHOMBUS = 4;// константы обозначающие тип фигур
        public bool DEBUG = true;

        public Form1()
        {
            InitializeComponent();
            field.InitializeContexts();
            field.MouseWheel += field_Mouse_Wheel;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            clear();
            // инициализация Glut 
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_MULTISAMPLE);
            Glut.glutDisplayFunc(Draw);
            Glut.glutIdleFunc(Draw);
            // очистка окна 
            Gl.glClearColor(255, 255, 255, 1);
            // установка порта вывода в соответствии с размерами элемента anT 
            Gl.glViewport(0, 0, field.Width, field.Height);
            Gl.glPointSize(5);
            Gl.glLineWidth(3f);
            Gl.glEnable(Gl.GL_POINT_SMOOTH);
            // настройка проекции 
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            // теперь необходимо корректно настроить 2D ортогональную проекцию 
            Glu.gluOrtho2D(0.0, (float)field.Width, 0.0, (float)field.Height);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glEnable(Gl.GL_POINT_SMOOTH);
            Gl.glHint(Gl.GL_POINT_SMOOTH_HINT, Gl.GL_NICEST);
            // Сглаживание линий
            Gl.glEnable(Gl.GL_LINE_SMOOTH);
            Gl.glHint(Gl.GL_LINE_SMOOTH_HINT, Gl.GL_NICEST);
            // Сглаживание полигонов    
            Gl.glEnable(Gl.GL_POLYGON_SMOOTH);
            Gl.glHint(Gl.GL_POLYGON_SMOOTH_HINT, Gl.GL_NICEST);
            cboxSelectedType.SelectedIndex = 0;
        }
        private void Draw()//отрисовка всех фигур
        {
            dynamic queue = new Queue(); // создать новую очередь
            queue.Enqueue(ShapesTree); // поместить в очередь первый уровень

            // очистка буфера цвета и буфера глубины 
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(255, 255, 255, 1);
            // очищение текущей матрицы 
            Gl.glLoadIdentity();
            // помещаем состояние матрицы в стек матриц 
            Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3d(0, 0, 0);
            Gl.glVertex2d(0.5, 0.5);
            Gl.glVertex2d(0.5, field.Height);
            Gl.glVertex2d(0.5, 0.5);
            Gl.glVertex2d(field.Width, 0.5);
            Gl.glEnd();
            if (ShapesTree.Count > 0)//существуют фигуры
            {
                int type;
                foreach (var temp in ShapesTree.Values)//для каждой существующей фигуры
                {
                    Gl.glPushMatrix();
                    //сохранить текущую матрицу всех остальных объектов в стек, использовать новую единичную матрицу для текущей фигуры temp
                    //провести некоторые действия с фигурой

                    Gl.glTranslated(temp._translateX, temp._translateY, 0.0f);
                    Gl.glTranslated(temp.center.X, temp.center.Y, 0.0f);//сместить по x y 
                    Gl.glScaled(temp.scale, temp.scale, 1);//масштабировать
                    Gl.glRotated(temp.angle, 0, 0, 1);//вращать
                    Gl.glTranslated(-temp.center.X, -temp.center.Y, 0.0f);
                    type = temp.type;
                    //задать цвет для текущей фигуры
                    Gl.glColor3ub(temp.color.R, temp.color.G, temp.color.B);
                    switch (type)
                    {
                        case PENTAGON:
                            //если это пятиугольник нарисовать
                            draw_Pentagon(temp);
                            break;
                        case ELLIPSE: //если это эллипс нарисовать
                            draw_Ellipse(temp);
                            break;
                        default://если это четырехугольник нарисовать
                            draw_Quadrangle(temp);
                            break;
                    }
                    //вынуть из стека матрицу всех остальных объектов и умножить ее на новую матрицу 
                    Gl.glPopMatrix();
                }
            }
            if (flag_input)//происходит ввод фигуры по точкам
                draw_stipple();//рисовать динамически

            // отрисовываем изменения 
            Gl.glFlush();
            // обновляем состояние элемента 
            field.Invalidate();

        }

        //процедура отрисовки любой фигуры, если происходит ввод
        private void draw_stipple()
        {
            //нарисовать две заданные точки
            Gl.glBegin(Gl.GL_POINTS);
            Gl.glColor3d(0, 0, 0);
            Gl.glVertex2d(temp_points[0].X, temp_points[0].Y);
            Gl.glVertex2d(temp_points[1].X, temp_points[1].Y);
            Gl.glEnd();

            Gl.glColor3d(0, 0, 0);
            Gl.glEnable(Gl.GL_LINE_STIPPLE);//включить режим рисования пунктиром
            Gl.glLineStipple(2, 0x0103);//размер пунктира
            if (count_click > 1)//задано точек больше 1
                switch (cboxSelectedType.Text.Length)
                {
                    case PARALLELOGRAM://нарисовать пунктиром будущий параллелограмм
                        Gl.glBegin(Gl.GL_LINE_LOOP);
                        Gl.glVertex2f(Gtemp.static_points[0].X, Gtemp.static_points[0].Y);
                        Gl.glVertex2f(Gtemp.static_points[1].X, Gtemp.static_points[1].Y);
                        Gl.glVertex2f(Gtemp.static_points[2].X, Gtemp.static_points[2].Y);
                        Gl.glVertex2f(Gtemp.static_points[3].X, Gtemp.static_points[3].Y);
                        Gl.glEnd();
                        break;
                    case RHOMBUS: //нарисовать пунктиром будущий ромб
                        Gl.glBegin(Gl.GL_LINE_LOOP);
                        Gl.glVertex2f(Gtemp.static_points[1].X, Gtemp.static_points[1].Y);
                        Gl.glVertex2f(Gtemp.static_points[3].X, Gtemp.static_points[3].Y);
                        Gl.glVertex2f(Gtemp.static_points[2].X, Gtemp.static_points[2].Y);
                        Gl.glVertex2f(Gtemp.static_points[4].X, Gtemp.static_points[4].Y);
                        Gl.glEnd();
                        break;
                    case PENTAGON://нарисовать пунктиром будущий пентагон
                        Gl.glBegin(Gl.GL_LINE_LOOP);
                        Gl.glVertex2f(Gtemp.static_points[1].X, Gtemp.static_points[1].Y);
                        Gl.glVertex2f(Gtemp.static_points[2].X, Gtemp.static_points[2].Y);
                        Gl.glVertex2f(Gtemp.static_points[3].X, Gtemp.static_points[3].Y);
                        Gl.glVertex2f(Gtemp.static_points[4].X, Gtemp.static_points[4].Y);
                        Gl.glVertex2f(Gtemp.static_points[5].X, Gtemp.static_points[5].Y);
                        Gl.glEnd();
                        break;
                    case ELLIPSE://нарисовать пунктиром будущий эллипс
                        double x, y;
                        int t;
                        Gl.glBegin(Gl.GL_LINE_LOOP);
                        for (t = 0; t <= 360; t += 1)
                        {
                            double angle = t * Math.PI / 180;
                            x = (temp_points[1].X - temp_points[0].X) * Math.Sin(angle) + temp_points[0].X;
                            y = (temp_points[2].Y - temp_points[0].Y) * Math.Cos(angle) + temp_points[0].Y;
                            Gl.glVertex2d(x, y);
                        }
                        Gl.glEnd();
                        break;
                    default:
                        Glut.glutSwapBuffers();
                        break;
                }
            Gl.glDisable(Gl.GL_LINE_STIPPLE);//выключить режим рисования пунктиром
        }

        //норма вектора
        private double norma(double x, double y)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        //очистить буффер точек
        private void clear()
        {
            for (int i = 0; i < 6; i++)
                temp_points[i] = new Point(-1, -1);
        }

        //заблокировать интерфейс
        private void lock_Interface(bool i)
        {
            cboxSelectedType.Enabled = i;
            cboxCountFigures.Enabled = i;
            btnCreateArbitrarily.Enabled = i;
            btn_Delete_Shape.Enabled = i;
        }

        //рисует четрыхугольник
        private void draw_Quadrangle(dynamic shape)
        {
            Point[] points = shape.static_points;//получить точки фигуры
            //нарисовать фигуру
            Gl.glBegin(Gl.GL_POLYGON);
            Gl.glVertex2f(points[0].X, points[0].Y);
            Gl.glVertex2f(points[1].X, points[1].Y);
            Gl.glVertex2f(points[2].X, points[2].Y);
            Gl.glVertex2f(points[3].X, points[3].Y);
            Gl.glEnd();
            if (shape.active)//фигура выбрана
            {
                //выделить ее контуром
                Gl.glColor3ub((byte)(255 - shape.color.R), (byte)(255 - shape.color.G), (byte)(255 - shape.color.B));
                Gl.glBegin(Gl.GL_LINE_LOOP);
                Gl.glVertex2f(points[3].X, points[3].Y);
                Gl.glVertex2f(points[0].X, points[0].Y);
                Gl.glVertex2f(points[1].X, points[1].Y);
                Gl.glVertex2f(points[2].X, points[2].Y);
                Gl.glEnd();
                //нарисовать точку центра
                Gl.glBegin(Gl.GL_POINTS);
                Gl.glColor3d(0, 0, 0);
                Gl.glVertex2d(shape.center.X, shape.center.Y);
                Gl.glEnd();
            }
        }
        //рисует пентагон
        private void draw_Pentagon(dynamic pentagon)
        {
            //нарисовать пентагон
            Point[] points = pentagon.static_points;
            Gl.glBegin(Gl.GL_POLYGON);
            Gl.glVertex2f(points[0].X, points[0].Y);
            Gl.glVertex2f(points[1].X, points[1].Y);
            Gl.glVertex2f(points[2].X, points[2].Y);
            Gl.glVertex2f(points[3].X, points[3].Y);
            Gl.glVertex2f(points[4].X, points[4].Y);
            Gl.glEnd();
            if (pentagon.active)//фигура выбрана
            {
                //выделить ее контуром
                Gl.glColor3ub((byte)(255 - pentagon.color.R), (byte)(255 - pentagon.color.G), (byte)(255 - pentagon.color.B));
                Gl.glBegin(Gl.GL_LINE_LOOP);
                Gl.glVertex2f(points[0].X, points[0].Y);
                Gl.glVertex2f(points[1].X, points[1].Y);
                Gl.glVertex2f(points[2].X, points[2].Y);
                Gl.glVertex2f(points[3].X, points[3].Y);
                Gl.glVertex2f(points[4].X, points[4].Y);
                Gl.glEnd();
                //нарисовать точку центра
                Gl.glBegin(Gl.GL_POINTS);
                Gl.glColor3d(0, 0, 0);
                Gl.glVertex2d(pentagon.center.X, pentagon.center.Y);
                Gl.glEnd();
            }

        }
        //нарисовать эллипс
        private void draw_Ellipse(dynamic ellipse)
        {
            Point[] points = ellipse.static_points; //получить точки
            double x, y;
            int t;
            Gl.glBegin(Gl.GL_POLYGON);
            //нарисовать эллипс по точкам
            for (t = 0; t <= 360 * (ellipse.R) / 80; t += 1)
            {
                double angle = t * Math.PI / 180;
                x = (points[0].X - ellipse.center.X) * Math.Sin(angle) + ellipse.center.X;
                y = (points[1].Y - ellipse.center.Y) * Math.Cos(angle) + ellipse.center.Y;
                Gl.glVertex2d(x, y);
            }
            Gl.glEnd();
            if (ellipse.active) //фигура выбрана
            {
                //выделить ее контуром
                Gl.glPointSize(3);
                Gl.glColor3ub((byte)(255 - ellipse.color.R), (byte)(255 - ellipse.color.G), (byte)(255 - ellipse.color.B));
                Gl.glBegin(Gl.GL_POINTS);
                for (t = 0; t <= 360; t += 1)
                {
                    double angle = t * Math.PI / 180;
                    x = (points[0].X - ellipse.center.X) * Math.Sin(angle) + ellipse.center.X;
                    y = (points[1].Y - ellipse.center.Y) * Math.Cos(angle) + ellipse.center.Y;
                    Gl.glVertex2d(x, y);
                }
                Gl.glEnd();
                Gl.glPointSize(5);
                Gl.glBegin(Gl.GL_POINTS);
                Gl.glColor3d(0, 0, 0);
                Gl.glVertex2d(ellipse.center.X, ellipse.center.Y);
                Gl.glEnd();
            }
        }
        //функция результат которой уникальный индекс зависящий от кол-ва фигур
        private uint index()
        {
            if (ShapesTree.Count > 0)//существуют фигуры
            {
                //найти такой индекс который не занят
                for (uint i = 1; i <= cboxCountFigures.Items.Count; i++)
                    if (!figuresList.ContainsKey(i)) return i;
                return (uint)cboxCountFigures.Items.Count + 1;
            }
            return 1;
        }

        //процедура создания параллелограмма
        private void create_Parallelogram(MouseEventArgs e)
        {
            //сохранить последнюю нажатую точку
            temp_points[count_click].X = e.X;
            temp_points[count_click].Y = field.Height - e.Y;
            try
            {
                //расчитать параметр t для уравнения прямых, что бы найти 4-ю точку 
                double t = (double)(temp_points[0].Y - temp_points[2].Y) / (temp_points[0].Y - temp_points[1].Y - temp_points[2].Y + temp_points[1].Y);
                temp_points[3].X = temp_points[0].X + Convert.ToInt32((temp_points[2].X - temp_points[1].X) * t);
                temp_points[3].Y = temp_points[0].Y + Convert.ToInt32((temp_points[2].Y - temp_points[1].Y) * t);
                //пересчитать t для нахождения центра
                t = (double)(temp_points[1].Y - temp_points[2].Y) / (temp_points[2].Y - temp_points[0].Y - temp_points[1].Y + temp_points[3].Y);
                Point center = new Point(temp_points[1].X + Convert.ToInt32((temp_points[1].X - temp_points[3].X) * t),
                temp_points[1].Y + Convert.ToInt32((temp_points[1].Y - temp_points[3].Y) * t));
                double R = norma(temp_points[1].X - center.X, temp_points[1].Y - center.Y);//расчитываем радиус описанной окружности
                uint q = index();//получить уникальный индекс для текущей фигуры
                ShapesTree.Add(R + q, new Parallelogram(temp_points, center, R, q));//добавить фигуру в контейнер
                figuresList.Add(q, R);
                cboxCountFigures.Items.Add(q);//добавить индекс фигуры в combobox
                cboxCountFigures.SelectedItem = q;//выбрать последнюю добавлную фигуру
            }
            catch (Exception)//если t -> inf 
            {
                exeption_label.Text = "Некоректно заданы точки, фигура не построена";
            }
        }
        //процедура создания пентагона
        private void create_Pentagon(MouseEventArgs e)
        {
            //сохранить последнюю нажатую точку
            temp_points[count_click].X = e.X;
            temp_points[count_click].Y = field.Height - e.Y;
            try
            {
                double u, t;
                double pX, pY;
                //расчитываем расстояние от центра до точки
                t = norma(e.X - temp_points[0].X, field.Height - e.Y - temp_points[0].Y);
                //определяем в какой плоскости рисовать 
                u = Math.Acos((temp_points[1].X - temp_points[0].X) / norma(temp_points[1].X - temp_points[0].X, temp_points[1].Y - temp_points[0].Y));
                u = (temp_points[0].Y > temp_points[1].Y ? -u : u);
                //находим радиус вектор и для каждой следущей точки поворачиваем этот вектор на 72 градуса относительно предыдущего
                for (int i = 1; i < 6; i++)
                {
                    pX = Math.Cos(u);
                    pY = Math.Sin(u);
                    //находим точку
                    temp_points[i].X = temp_points[0].X + Convert.ToInt32(pX * t);
                    temp_points[i].Y = temp_points[0].Y + Convert.ToInt32(pY * t);
                    //для следующей точки увеличим угол
                    u += Math.PI * 0.4;
                }
                //сохраняем центр
                Point center = temp_points[0];
                for (int i = 0; i < 5; i++)
                    temp_points[i] = temp_points[i + 1];
                double R = norma(temp_points[0].X - center.X, temp_points[0].Y - center.Y);//расчитываем радиус описанной окружности
                uint q = index();//получаем уникальный индекс для текущей фигуры
                ShapesTree.Add(R + q, new Pentagon(temp_points, center, R, q));//добавляем фигуру в контейнер
                figuresList.Add(q, R);
                cboxCountFigures.Items.Add(q);//добавить индекс фигуры в combobox
                cboxCountFigures.SelectedItem = q;//выбрать последнюю добавлную фигуру
            }
            catch (Exception)//если t -> inf 
            {
                exeption_label.Text = "Некоректно заданы точки, фигура не построена";
            }
        }
        //процедура создания ромба
        private void create_Rhombus(MouseEventArgs e)
        {
            Point center = new Point(temp_points[0].X, temp_points[0].Y); //выделить центр
            temp_points[0] = temp_points[1];
            temp_points[1].X = e.X;
            temp_points[1].Y = field.Height - e.Y;
            double u, t;
            double pX, pY;
            try
            {
                //вычисляем угол для нахождения диагонали
                u = Math.Acos((temp_points[0].X - center.X) / norma(temp_points[0].X - center.X, temp_points[0].Y - center.Y));
                u = (center.Y > temp_points[0].Y ? -u : u) + Math.PI / 2;
                pX = Math.Cos(u);
                pY = Math.Sin(u);
                //находим норму вектора для нахождения последних точек
                t = norma(e.X - center.X, field.Height - e.Y - center.Y);
                temp_points[2].X = center.X + center.X - temp_points[0].X;
                temp_points[2].Y = center.Y + center.Y - temp_points[0].Y;
                temp_points[3].X = center.X + Convert.ToInt32(pX * t);
                temp_points[3].Y = center.Y + Convert.ToInt32(pY * t);
                temp_points[1].X = center.X + Convert.ToInt32(pX * (-t));
                temp_points[1].Y = center.Y + Convert.ToInt32(pY * (-t));
                //вычисляем радиус описанной окружности
                double R = (norma(temp_points[1].X - center.X, temp_points[1].Y - center.Y) < norma(temp_points[0].X - center.X, temp_points[0].Y - center.Y)) ? norma(temp_points[0].X - center.X, temp_points[0].Y - center.Y) : norma(temp_points[1].X - center.X, temp_points[1].Y - center.Y);
                uint q = index();//получаем уникальный индекс
                ShapesTree.Add(R + q, new Rhombus(temp_points, center, R, q)); //добавляем в контейнер
                figuresList.Add(q, R);
                cboxCountFigures.Items.Add(q);
                cboxCountFigures.SelectedItem = q;
            }
            catch (Exception) //t ->inf
            {
                exeption_label.Text = "Некорректно заданы точки, фигура не построена";
            }
        }
        //процедура создания эллипса
        private void create_Ellipse(MouseEventArgs e)
        {
            Point center = new Point(temp_points[0].X, temp_points[0].Y); //выбираем центр
            temp_points[0] = temp_points[1];
            temp_points[1].X = e.X;
            temp_points[1].Y = field.Height - e.Y;
            try
            {
                //вычисляем радиус описанной окружности
                double R = norma(temp_points[1].X - center.X, temp_points[1].Y - center.Y) >= norma(temp_points[0].X - center.X, temp_points[0].Y - center.Y) ? norma(temp_points[1].X - center.X, temp_points[1].Y - center.Y) : norma(temp_points[0].X - center.X, temp_points[0].Y - center.Y);
                uint q = index();//получаем уникальный индекс
                ShapesTree.Add(R + q, new Ellipse(temp_points, center, R, q)); //добавляем в контейнер
                figuresList.Add(q, R);
                cboxCountFigures.Items.Add(q);
                cboxCountFigures.SelectedItem = q;
            }
            catch (Exception)
            {
                exeption_label.Text = "Некорректно заданы точки, фигура не построена";
            }
        }
        //обработчик нажатия клавиш
        private void field_Key_Down(object sender, KeyEventArgs e)
        {
            if (ShapesTree.Count == 0) return;//существуют фигуры
            dynamic temp = ShapesTree[Convert.ToDouble(figuresList.Get(pointer_shape) + pointer_shape)];//для выбранной фигуры
            switch (e.KeyCode)
            {
                // W A S D отвечают за перемещение по x и y 
                case Keys.W:
                    temp.setTranslate(0, temp.move_speed * 0.03);
                    break;
                case Keys.S:
                    temp.setTranslate(0, -temp.move_speed * 0.03);
                    break;
                case Keys.A:
                    temp.setTranslate(-temp.move_speed * 0.03, 0);
                    break;
                case Keys.D:
                    temp.setTranslate(temp.move_speed * 0.03, 0);
                    break;
                // Е и Q отвечают за поворот
                case Keys.E:
                    temp.toRotate(1.0f);
                    break;
                case Keys.Q:
                    temp.toRotate(-1.0f);
                    break;
                default:
                    break;
            }
        }

        //обработчик движения мыши
        private void field_Mouse_Move(object sender, MouseEventArgs e)
        {
            double t, u, pX, pY;
            try
            {
                if (flag_input)//ввод фигуры мышью
                    if (count_click > 1)//точек 2 или больше
                        //динамически изменять последние точки в зависимости от положения мыши
                        switch (cboxSelectedType.Text.Length)
                        {
                            case PARALLELOGRAM:
                                temp_points[2].X = e.X;
                                temp_points[2].Y = field.Height - e.Y;
                                t = (double)(temp_points[0].Y - temp_points[2].Y) / (temp_points[0].Y - temp_points[1].Y - temp_points[2].Y + temp_points[1].Y);
                                temp_points[3].X = temp_points[0].X + Convert.ToInt32((temp_points[2].X - temp_points[1].X) * t);
                                temp_points[3].Y = temp_points[0].Y + Convert.ToInt32((temp_points[2].Y - temp_points[1].Y) * t);
                                break;
                            case PENTAGON:
                                t = norma(e.X - temp_points[0].X, field.Height - e.Y - temp_points[0].Y);
                                u = Math.Acos((temp_points[1].X - temp_points[0].X) / norma(temp_points[1].X - temp_points[0].X, temp_points[1].Y - temp_points[0].Y));
                                u = temp_points[0].Y > temp_points[1].Y ? -u : u;
                                for (int i = 1; i < 6; i++)
                                {
                                    pX = Math.Cos(u);
                                    pY = Math.Sin(u);
                                    temp_points[i].X = temp_points[0].X + Convert.ToInt32(pX * t);
                                    temp_points[i].Y = temp_points[0].Y + Convert.ToInt32(pY * t);
                                    u += Math.PI * 0.4;
                                }
                                break;
                            case RHOMBUS:
                                t = norma(e.X - temp_points[0].X, field.Height - e.Y - temp_points[0].Y);
                                u = Math.Acos((temp_points[1].X - temp_points[0].X) / norma(temp_points[1].X - temp_points[0].X, temp_points[1].Y - temp_points[0].Y));
                                u = (temp_points[0].Y > temp_points[1].Y ? -u : u) + Math.PI / 2;
                                pX = Math.Cos(u);
                                pY = Math.Sin(u);

                                temp_points[2].X = temp_points[0].X + temp_points[0].X - temp_points[1].X;
                                temp_points[2].Y = temp_points[0].Y + temp_points[0].Y - temp_points[1].Y;

                                temp_points[3].X = temp_points[0].X + Convert.ToInt32(pX * t);
                                temp_points[3].Y = temp_points[0].Y + Convert.ToInt32(pY * t);

                                temp_points[4].X = temp_points[0].X + Convert.ToInt32(pX * (-t));
                                temp_points[4].Y = temp_points[0].Y + Convert.ToInt32(pY * (-t));
                                break;
                            case ELLIPSE:
                                temp_points[2].X = e.X;
                                temp_points[2].Y = field.Height - e.Y;
                                break;
                            default:
                                break;
                        }
                Gtemp.static_points = temp_points;//сохранить измененные точки
            }
            catch (Exception)
            {
                Gtemp.static_points = temp_points;
            }

        }

        //обработчик клика мышью
        private void field_Mouse_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)//была нажата левая кнопка мыши
            {
                if (flag_input)//ввод фигуры мышью
                {
                    if (count_click < 2)//точек меньше 2
                    {
                        //сохранить точки
                        temp_points[count_click].X = e.X;
                        temp_points[count_click].Y = field.Height - e.Y;
                        count_click++;
                    }
                    else
                    {
                        //создать фигуру
                        switch (cboxSelectedType.Text.Length)
                        {
                            case PARALLELOGRAM:
                                create_Parallelogram(e);
                                break;
                            case RHOMBUS:
                                create_Rhombus(e);
                                break;
                            case PENTAGON:
                                create_Pentagon(e);
                                break;
                            case ELLIPSE:
                                create_Ellipse(e);
                                break;


                            default:
                                exeption_label.Text = "Ошибка не выбран тип создаваемой фигуры";
                                break;
                        }
                        //сброс 
                        count_click = 0;
                        flag_input = false;
                        lock_Interface(true);
                        clear();
                    }

                }

            }
        }

        //обработчик вращения колесика мыши
        private void field_Mouse_Wheel(object sender, MouseEventArgs e)
        {
            if (ShapesTree.Count == 0) return;//существуют фигуры
            dynamic temp = ShapesTree[Convert.ToDouble(figuresList.Get(pointer_shape)) + pointer_shape];//выбрать текущую
            if (e.Delta > 0)//колесико крутится вверх 
                temp.scale = -0.05;//уменить масштаб
            else
                temp.scale = 0.05;//увеличить масштаб
        }

        //обработчик таймера
        private void timer1_Tick(object sender, EventArgs e)
        {
            Draw();//рисовать все
        }

        //изменение скорости вращения
        private void rotating_Speed_Change(object sender, EventArgs e)
        {
            if (ShapesTree.Count == 0) return;//существуют фигуры
            ShapesTree[Convert.ToDouble(figuresList.Get(pointer_shape)) + pointer_shape].rotating_speed = barRotatingSpeed.Value;//выбрать текущую и изменить ее скорость
            field.Focus();
        }

        //изменение скорости движения
        private void move_Speed_Change(object sender, EventArgs e)
        {
            if (ShapesTree.Count == 0) return;
            ShapesTree[Convert.ToDouble(figuresList.Get(pointer_shape)) + pointer_shape].move_speed = barMoveSpeed.Value;//выбрать текущую и изменить ее скорость
            field.Focus();
        }

        //изменение цвета фигуры
        private void btn_Set_Color_Click(object sender, EventArgs e)
        {
            if (ShapesTree.Count == 0) return;//существуют фигуры
            dynamic temp = ShapesTree[Convert.ToDouble(figuresList.Get(pointer_shape)) + pointer_shape];//выбрать текущую 
            //открыть палитру цветов
            ColorDialog MyDialog = new ColorDialog();
            MyDialog.AllowFullOpen = false;
            MyDialog.ShowHelp = true;
            MyDialog.Color = temp.color;//выбрать новый цвет
            if (MyDialog.ShowDialog() == DialogResult.OK)
            {
                btnSetColor.BackColor = MyDialog.Color;
                temp.color = MyDialog.Color;
            }
            //закрыть диалог
            field.Focus();
        }
        //вывод фигур в файл
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileStream file = new FileStream("shapes.txt", FileMode.Create);
            StreamWriter writer = new StreamWriter(file);
            dynamic queue = new Queue();
            dynamic temp;
            queue.Enqueue(ShapesTree);
            while (queue.Count != 0) // пока очередь не пуста
            {
                if (queue.Peek().Left != null)
                    queue.Enqueue(queue.Peek().Left);
                if (queue.Peek().Right != null)
                    queue.Enqueue(queue.Peek().Right);
                temp = queue.Dequeue().Data;
                writer.Write("#" + Environment.NewLine + temp.type + Environment.NewLine);
                foreach (Point p in temp.static_points)
                    writer.Write(p.X + " " + p.Y + " ");
                writer.Write(Environment.NewLine + temp.center.X + " " + temp.center.Y + Environment.NewLine + temp.angle + Environment.NewLine
                    + temp.move_speed + " " + temp.rotating_speed + Environment.NewLine + temp.color.R + " " + temp.color.G + " " + temp.color.B
                    + Environment.NewLine + temp.scale + Environment.NewLine + temp._translateX + " " + temp._translateY + Environment.NewLine);
            }
            writer.Close();
            file.Close();
        }
        //загрузка фигур из файла
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileStream file = new FileStream("shapes.txt", FileMode.Open);
            StreamReader reader = new StreamReader(file);
            string line;
            Point center;
            double R;
            uint q;
            int type;
            while ((line = reader.ReadLine()) != null)
            {
                count_click = 4;
                if (line != "#")
                    return;
                line = reader.ReadLine();
                type = Convert.ToInt32(line);
                line = reader.ReadLine();
                string[] s = line.Split(' ');
                for (int C = 0; s[C] != ""; C++)
                    if (C % 2 == 0)
                        temp_points[C / 2].X = Convert.ToInt32(s[C]);
                    else
                        temp_points[(C - 1) / 2].Y = Convert.ToInt32(s[C]);
                line = reader.ReadLine();
                switch (type)
                {
                    case PARALLELOGRAM:
                        center = new Point(Convert.ToInt32(line.Split(' ')[0]), Convert.ToInt32(line.Split(' ')[1]));
                        R = norma(temp_points[1].X - center.X, temp_points[1].Y - center.Y);
                        q = index();
                        ShapesTree.Add(R + q, new Parallelogram(temp_points, center, R, q));
                        figuresList.Add(q, R);
                        cboxCountFigures.Items.Add(q);
                        cboxCountFigures.SelectedItem = q;
                        Parallelogram p = ShapesTree[figuresList.Get(q) + q];
                        p.angle = Convert.ToDouble(reader.ReadLine());
                        line = reader.ReadLine();
                        p.rotating_speed = Convert.ToInt32(line.Split(' ')[1]);
                        p.move_speed = Convert.ToInt32(line.Split(' ')[0]);
                        line = reader.ReadLine();
                        p.color = Color.FromArgb(Convert.ToInt32(line.Split(' ')[0]), Convert.ToInt32(line.Split(' ')[1]), Convert.ToInt32(line.Split(' ')[2]));
                        p.scale = Convert.ToDouble(reader.ReadLine()) - 1;
                        line = reader.ReadLine();
                        p.setTranslate(Convert.ToDouble(line.Split(' ')[0]), Convert.ToDouble(line.Split(' ')[1]));
                        break;
                    case PENTAGON:
                        center = new Point(Convert.ToInt32(line.Split(' ')[0]), Convert.ToInt32(line.Split(' ')[1]));
                        R = norma(temp_points[1].X - center.X, temp_points[1].Y - center.Y);
                        q = index();
                        ShapesTree.Add(R + q, new Pentagon(temp_points, center, R, q));
                        figuresList.Add(q, R);
                        cboxCountFigures.Items.Add(q);
                        cboxCountFigures.SelectedItem = q;
                        Pentagon pentagon = ShapesTree[Convert.ToDouble(figuresList.Get(q)) + q];
                        pentagon.angle = Convert.ToDouble(reader.ReadLine());
                        line = reader.ReadLine();
                        pentagon.rotating_speed = Convert.ToInt32(line.Split(' ')[1]);
                        pentagon.move_speed = Convert.ToInt32(line.Split(' ')[0]);
                        line = reader.ReadLine();
                        pentagon.color = Color.FromArgb(Convert.ToInt32(line.Split(' ')[0]), Convert.ToInt32(line.Split(' ')[1]), Convert.ToInt32(line.Split(' ')[2]));
                        pentagon.scale = Convert.ToDouble(reader.ReadLine()) - 1;
                        line = reader.ReadLine();
                        pentagon.setTranslate(Convert.ToDouble(line.Split(' ')[0]), Convert.ToDouble(line.Split(' ')[1]));
                        break;
                    case RHOMBUS:
                        center = new Point(Convert.ToInt32(line.Split(' ')[0]), Convert.ToInt32(line.Split(' ')[1]));
                        R = norma(temp_points[1].X - center.X, temp_points[1].Y - center.Y);
                        q = index();
                        ShapesTree.Add(R + q, new Rhombus(temp_points, center, R, q));
                        figuresList.Add(q, R);
                        cboxCountFigures.Items.Add(q);
                        cboxCountFigures.SelectedItem = q;
                        Rhombus rhombus = ShapesTree[Convert.ToDouble(figuresList.Get(q)) + q];
                        rhombus.angle = Convert.ToDouble(reader.ReadLine());
                        line = reader.ReadLine();
                        rhombus.rotating_speed = Convert.ToInt32(line.Split(' ')[1]);
                        rhombus.move_speed = Convert.ToInt32(line.Split(' ')[0]);
                        line = reader.ReadLine();
                        rhombus.color = Color.FromArgb(Convert.ToInt32(line.Split(' ')[0]), Convert.ToInt32(line.Split(' ')[1]), Convert.ToInt32(line.Split(' ')[2]));
                        rhombus.scale = Convert.ToDouble(reader.ReadLine()) - 1;
                        line = reader.ReadLine();
                        rhombus.setTranslate(Convert.ToDouble(line.Split(' ')[0]), Convert.ToDouble(line.Split(' ')[1]));
                        break;
                    case ELLIPSE:
                        center = new Point(Convert.ToInt32(line.Split(' ')[0]), Convert.ToInt32(line.Split(' ')[1]));
                        R = norma(temp_points[1].X - center.X, temp_points[1].Y - center.Y) >= norma(temp_points[0].X - center.X, temp_points[0].Y - center.Y) ? norma(temp_points[1].X - center.X, temp_points[1].Y - center.Y) : norma(temp_points[0].X - center.X, temp_points[0].Y - center.Y);
                        q = index();
                        ShapesTree.Add(R + q, new Ellipse(temp_points, center, R, q));
                        figuresList.Add(q, R);
                        cboxCountFigures.Items.Add(q);
                        cboxCountFigures.SelectedItem = q;
                        Ellipse el = ShapesTree[Convert.ToDouble(figuresList.Get(q)) + q];
                        el.angle = Convert.ToDouble(reader.ReadLine());
                        line = reader.ReadLine();
                        el.rotating_speed = Convert.ToInt32(line.Split(' ')[1]);
                        el.move_speed = Convert.ToInt32(line.Split(' ')[0]);
                        line = reader.ReadLine();
                        el.color = Color.FromArgb(Convert.ToInt32(line.Split(' ')[0]), Convert.ToInt32(line.Split(' ')[1]), Convert.ToInt32(line.Split(' ')[2]));
                        el.scale = Convert.ToDouble(reader.ReadLine()) - 1;
                        line = reader.ReadLine();
                        el.setTranslate(Convert.ToDouble(line.Split(' ')[0]), Convert.ToDouble(line.Split(' ')[1]));
                        break;
                    default:
                        exeption_label.Text = "Ошибка файл содержит не определенный тип фигуры " + type;
                        break;
                }
                clear();
            }
            reader.Close();
            file.Close();
        }


        //обработчик включения ввода по точкам
        private void enable_Input_Shape(object sender, EventArgs e)
        {
            exeption_label.Text = "";
            flag_input = true;
            lock_Interface(false);
            field.Focus();
        }
        //обраточик смены выбранной фигуры
        private void cbox_Selected_Item_Change(object sender, EventArgs e)
        {
            dynamic temp = ShapesTree[Convert.ToDouble(figuresList.Get(pointer_shape)) + pointer_shape];//выбрать новую
            //установить настройки баров для текущей фигуры
            temp.active = false;
            pointer_shape = Convert.ToUInt32(cboxCountFigures.Text);
            temp = ShapesTree[Convert.ToDouble(figuresList.Get(pointer_shape)) + pointer_shape];
            temp.active = true;
            barMoveSpeed.Value = temp.move_speed;
            barRotatingSpeed.Value = temp.rotating_speed;
            btnSetColor.BackColor = temp.color;
            field.Focus();

        }
        //обработчик удаления фигуры
        private void btn_Delete_Sel_Shape(object sender, EventArgs e)
        {
            if (ShapesTree.Count == 0) return;//существуют фигуры
            ShapesTree.Remove(figuresList.Get(pointer_shape) + pointer_shape);//удалить выбранную фигуру
            figuresList.Remove(pointer_shape, null);
            cboxCountFigures.Items.RemoveAt(cboxCountFigures.SelectedIndex);
            long t = ShapesTree.Count;
            pointer_shape = (ShapesTree.Count > 0) ? (uint)cboxCountFigures.Items[(int)ShapesTree.Count - 1] : 1;
            cboxCountFigures.SelectedItem = pointer_shape;
            field.Focus();
        }

    }
}