﻿using System;
using System.Collections.Generic;
using System.Drawing;

using Sharp3D.Math.Core;

namespace treeDiM.StackBuilder.Basics
{
    public class PalletProperties : ItemBaseNamed
    {
        public PalletProperties(Document document, string typeName, double length, double width, double height)
            : base(document)
        {
            _typeName = typeName;
            _length = length;
            _width = width;
            _height = height;
        }

        public double[] Dimensions => new double[] { _length, _width, _height };
        public double Length
        {
            get { return _length; }
            set { _length = value; Modify(); }
        }
        public double Width
        {
            get { return _width; }
            set { _width = value; Modify(); }
        }
        public double Height
        {
            get { return _height; }
            set { _height = value; Modify(); }
        }
        public double Weight
        {
            get { return _weight; }
            set { _weight = value; Modify(); }
        }
        public double AdmissibleLoadWeight
        {
            get { return _admissibleLoadWeight; }
            set { _admissibleLoadWeight = value; Modify(); }
        }
        public double AdmissibleLoadHeight
        {
            get { return _admissibleLoadHeight; }
            set { _admissibleLoadHeight = value; Modify(); }
        }
        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; Modify(); }
        }
        public Color Color
        {
            get { return _color; }
            set { _color = value; Modify(); }
        }
        public BBox3D BoundingBox => new BBox3D(Vector3D.Zero, new Vector3D(_length, _width, _height));

        public override string ToString()
        {
            var sBuilder = new System.Text.StringBuilder();
            sBuilder.Append(base.ToString());
            sBuilder.Append($"PalletProperties => Type {_typeName} Length {_length} Width {_width} Height {_height}");
            return sBuilder.ToString();
        }

        #region Non-Public Members

        double _length, _width, _height;
        double _weight;
        double _admissibleLoadWeight, _admissibleLoadHeight;
        Color _color = Color.Yellow;
        string _typeName = "Block";
        
        #endregion

    }
}
