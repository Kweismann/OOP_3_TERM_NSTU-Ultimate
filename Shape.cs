﻿using System;
using System.Drawing;

namespace Shapes
{
    public class Shape
    {
        private double _scale;
        public double _translateX, _translateY;
        public double R { get; set; }
        public Color color { set; get; }
        public Point center { set; get; }
        public double angle { set; get; }
        public bool active { set; get; }
        public int move_speed { set; get; }
        public int rotating_speed { set; get; }
        public uint index { set; get; }
        public Point[] static_points { set; get; }
        public double scale { set { _scale += value; } get { return _scale; } }
        //процедура отвечающая за вращение фигуры на угол angle
        public void toRotate(double angle)
        {
            this.angle += angle * Math.PI / 10 * rotating_speed;
        }
        //процедура отвечающая за смещение фигуры на dx и dy
        public void setTranslate(double dx, double dy)
        {
            _translateX += dx;
            _translateY += dy;
        }
    }
    //класс Parallelogram наследник от Shape
    public class Parallelogram : Shape
    {
        public int type { get; }
        //конструктор класса
        public Parallelogram(Point[] new_points, Point c, double R, uint index)
        {
            static_points = new Point[4];
            static_points[0].X = new_points[0].X;
            static_points[0].Y = new_points[0].Y;
            static_points[1].X = new_points[1].X;
            static_points[1].Y = new_points[1].Y;
            static_points[2].X = new_points[2].X;
            static_points[2].Y = new_points[2].Y;
            static_points[3].X = new_points[3].X;
            static_points[3].Y = new_points[3].Y;
            angle = 0;
            type = 14;
            scale = 1;
            center = c;
            this.R = R;
            active = false;
            move_speed = 2;
            this.index = index;
            rotating_speed = 1;
            _translateX = 0;
            _translateY = 0;
            Random rand = new Random();
            color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        }
    }

    //класс Pentagon наследник от Shape
    public class Pentagon : Shape
    {
        public int type { get; }

        //конструктор класса
        public Pentagon(Point[] new_points, Point center, double R, uint index)
        {
            static_points = new Point[5];
            static_points[0].X = new_points[0].X;
            static_points[0].Y = new_points[0].Y;

            static_points[1].X = new_points[1].X;
            static_points[1].Y = new_points[1].Y;

            static_points[2].X = new_points[2].X;
            static_points[2].Y = new_points[2].Y;

            static_points[3].X = new_points[3].X;
            static_points[3].Y = new_points[3].Y;

            static_points[4].X = new_points[4].X;
            static_points[4].Y = new_points[4].Y;


            type = 8;
            angle = 0;
            scale = 1;
            this.R = R;
            active = false;
            move_speed = 2;
            this.index = index;
            rotating_speed = 1;
            this.center = center;
            Random rand = new Random();
            color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        }
    }

    //класс Rhombus наследник от Shape
    public class Rhombus : Shape
    {
        public int type { get; }
        //конструктор класса
        public Rhombus(Point[] new_points, Point c, double R, uint index)
        {
            static_points = new Point[4];
            static_points[0].X = new_points[0].X;
            static_points[0].Y = new_points[0].Y;

            static_points[1].X = new_points[1].X;
            static_points[1].Y = new_points[1].Y;

            static_points[2].X = new_points[2].X;
            static_points[2].Y = new_points[2].Y;

            static_points[3].X = new_points[3].X;
            static_points[3].Y = new_points[3].Y;


            type = 4;
            angle = 0;
            scale = 1;
            center = c;
            this.R = R;
            active = false;
            move_speed = 2;
            this.index = index;
            rotating_speed = 1;
            Random rand = new Random();
            color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        }
    }
    //класс Ellipse наследник от Shape
    public class Ellipse : Shape
    {
        public int type { get; }
        //конструктор класса
        public Ellipse(Point[] points, Point c, double R, uint i)
        {
            static_points = new Point[2];
            static_points[0] = points[0];
            static_points[1] = points[1];

            this.R = R;
            index = i;
            type = 6;
            angle = 0;
            scale = 1;
            center = c;
            active = false;
            move_speed = 2;
            rotating_speed = 1;
            _translateX = 0;
            _translateY = 0;

            Random rand = new Random();
            color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        }
    }
}
