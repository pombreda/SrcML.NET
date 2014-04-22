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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ABB.SrcML.Data {
    public class Statement {
        private List<Statement> childStatementsList;
        
        public Statement() {
            childStatementsList = new List<Statement>();
            ChildStatements = new ReadOnlyCollection<Statement>(childStatementsList);
        }
        
        public ReadOnlyCollection<Statement> ChildStatements { get; private set; }
        public Statement ParentStatement { get; set; }
        public SrcMLLocation Location { get; set; }
        public Language ProgrammingLanguage { get; set; }
        public Expression Content {get; set;}

        /// <summary>
        /// Adds the given Statement to the ChildStatements collection.
        /// </summary>
        /// <param name="child">The Statement to add.</param>
        public virtual void AddChildStatement(Statement child) {
            if(child == null) { throw new ArgumentNullException("child"); }

            child.ParentStatement = this;
            childStatementsList.Add(child);
        }

        /// <summary>
        /// Adds the given Statements to the ChildStatements collection.
        /// </summary>
        /// <param name="children">The Statements to add.</param>
        public void AddChildStatements(IEnumerable<Statement> children) {
            foreach(var child in children) {
                AddChildStatement(child);
            }
        }

        /// <summary>
        /// Gets all of the parents of this statement
        /// </summary>
        /// <returns>The parents of this statement</returns>
        public IEnumerable<Statement> GetAncestors() {
            return GetAncestorsAndStartingPoint(this.ParentStatement);
        }

        /// <summary>
        /// Gets all of the parents of type <typeparamref name="T"/> of this statement.
        /// </summary>
        /// <typeparam name="T">The type to filter the parent statements by</typeparam>
        /// <returns>The parents of type <typeparamref name="T"/></returns>
        public IEnumerable<T> GetAncestors<T>() where T : Statement {
            return GetAncestorsAndStartingPoint(this.ParentStatement).OfType<T>();
        }

        /// <summary>
        /// Gets all of parents of this statement as well as this statement.
        /// </summary>
        /// <returns>This statement followed by its parents</returns>
        public IEnumerable<Statement> GetAncestorsAndSelf() {
            return GetAncestorsAndStartingPoint(this);
        }

        /// <summary>
        /// Gets all of the parents of this statement as well as the statement itself where the type is <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to filter the parent statements by</typeparam>
        /// <returns>This statement followed by its parent statements where the type is <typeparamref name="T"/></returns>
        public IEnumerable<T> GetAncestorsAndSelf<T>() where T : Statement {
            return GetAncestorsAndStartingPoint(this).OfType<T>();
        }

        /// <summary>
        /// Gets all of the descendant statements of this statement. This is every statement that is rooted at this statement.
        /// </summary>
        /// <returns>The descendants of this statement</returns>
        public IEnumerable<Statement> GetDescendants() {
            return GetDescendants(this, false);
        }

        /// <summary>
        /// Gets all of the descendant statements of this statement where the type is <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to filter the descendant statements by</typeparam>
        /// <returns>The descendants of type <typeparamref name="T"/> of this statement</returns>
        public IEnumerable<T> GetDescendants<T>() where T : Statement {
            return GetDescendants(this, false).OfType<T>();
        }

        /// <summary>
        /// Gets all of the descendants of this statement as well as the statement itself.
        /// </summary>
        /// <returns>This statement, followed by all of its descendants</returns>
        public IEnumerable<Statement> GetDescendantsAndSelf() {
            return GetDescendants(this, true);
        }

        /// <summary>
        /// Gets all of the descendants of this statement as well as the statement itself where the type is <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to filter the descendant statements by</typeparam>
        /// <returns>This statement and its descendants where the type is <typeparamref name="T"/></returns>
        public IEnumerable<T> GetDescendantsAndSelf<T>() where T : Statement {
            return GetDescendants(this, true).OfType<T>();
        }

        /// <summary>
        /// Gets a statement and all of its ancestors
        /// </summary>
        /// <param name="startingPoint">The first node to return</param>
        /// <returns>The <paramref name="startingPoint"/> and all of it ancestors</returns>
        private static IEnumerable<Statement> GetAncestorsAndStartingPoint(Statement startingPoint) {
            var current = startingPoint;
            while(null != current) {
                yield return current;
                current = current.ParentStatement;
            }
        }

        /// <summary>
        /// Gets the <paramref name="startingPoint"/> (if <paramref name="returnStartingPoint"/> is true) and all of the descendants of the <paramref name="startingPoint"/>.
        /// </summary>
        /// <param name="startingPoint">The starting point</param>
        /// <param name="returnStartingPoint">If true, return the starting point first. Otherwise, just return  the descendants.</param>
        /// <returns><paramref name="startingPoint"/> (if <paramref name="returnStartingPoint"/> is true) and its descendants</returns>
        private static IEnumerable<Statement> GetDescendants(Statement startingPoint, bool returnStartingPoint) {
            if(returnStartingPoint) {
                yield return startingPoint;
            }

            foreach(var statement in startingPoint.ChildStatements) {
                foreach(var descendant in GetDescendants(statement, true)) {
                    yield return descendant;
                }
            }
        }
    }
}