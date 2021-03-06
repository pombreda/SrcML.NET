﻿/******************************************************************************
 * Copyright (c) 2014 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Patrick Francis (ABB Group) - initial API, implementation, & documentation
 *    Vinay Augustine (ABB Group) - initial API, implementation, & documentation
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ABB.SrcML.Data
{
    /// <summary>
    /// Represents a using block statement in C#.
    /// These are of the form: 
    /// <code> using(Foo f = new Foo()) { ... } </code>
    /// Note that this is different from a using directive, e.g. <code>using System.Text;</code>
    /// </summary>
    public class UsingBlockStatement : BlockStatement {
        private Expression initExpression;

        /// <summary> The XML name for UsingBlockStatement </summary>
        public new const string XmlName = "UsingBlock";

        /// <summary> XML Name for <see cref="Initializer" /> </summary>
        public const string XmlInitializerName = "Initializer";

        /// <summary> The intialization expression for the using block. </summary>
        public Expression Initializer {
            get { return initExpression; }
            set {
                initExpression = value;
                if(initExpression != null) {
                    initExpression.ParentStatement = this;
                }
            }
        }

        /// <summary>
        /// Instance method for getting <see cref="UsingBlockStatement.XmlName"/>
        /// </summary>
        /// <returns>Returns the XML name for UsingBlockStatement</returns>
        public override string GetXmlName() { return UsingBlockStatement.XmlName; }

        /// <summary>
        /// Processes the child of the current reader position into a child of this object.
        /// </summary>
        /// <param name="reader">The XML reader</param>
        protected override void ReadXmlChild(XmlReader reader) {
            if(XmlInitializerName == reader.Name) {
                Initializer = XmlSerialization.ReadChildExpression(reader);
            } else {
                base.ReadXmlChild(reader);
            }
        }

        /// <summary>
        /// Writes the contents of this object to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The XML writer to write to</param>
        protected override void WriteXmlContents(XmlWriter writer) {
            if(null != Initializer) {
                XmlSerialization.WriteElement(writer, Initializer, XmlInitializerName);
            }
            base.WriteXmlContents(writer);
        }

        /// <summary>
        /// Returns all the expressions within this statement.
        /// </summary>
        public override IEnumerable<Expression> GetExpressions() {
            if(Initializer != null) {
                yield return Initializer;
            }
        }

        /// <summary>
        /// Returns a string representation of this statement.
        /// </summary>
        public override string ToString() {
            return string.Format("using({0})", Initializer);
        }
    }
}
