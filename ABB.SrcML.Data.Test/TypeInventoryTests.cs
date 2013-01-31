﻿/******************************************************************************
 * Copyright (c) 2013 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Vinay Augustine (ABB Group) - initial API, implementation, & documentation
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Xml.Linq;

namespace ABB.SrcML.Data.Test {
    [TestFixture]
    public class TypeInventoryTests {
        private SrcMLFileUnitSetup fileSetup;

        [TestFixtureSetUp]
        public void ClassSetup() {
            fileSetup = new SrcMLFileUnitSetup(Language.Java);
        }

        [Test]
        public void BasicParentTest_Java() {
            // # A.java
            // class A implements B {
            // }
            string a_xml = @"<class>class <name>A</name> <super><implements>implements <name>B</name></implements></super> <block>{
}</block></class>";
            // # B.java
            // class B {
            // }
            string b_xml = @"<class>class <name>B</name> <block>{
}</block></class>";

            // # C.java
            // class C {
            //     A a;
            // }
            string c_xml = @"<class>class <name>C</name> <block>{
	<decl_stmt><decl><type><name>A</name></type> <name>a</name></decl>;</decl_stmt>
}</block></class>";

            var fileUnitA = fileSetup.GetFileUnitForXmlSnippet(a_xml, "A.java");
            var fileUnitB = fileSetup.GetFileUnitForXmlSnippet(b_xml, "B.java");
            var fileUnitC = fileSetup.GetFileUnitForXmlSnippet(c_xml, "C.java");

            AbstractCodeParser parser = new JavaCodeParser();
            TypeInventory inventory = new TypeInventory();

            var scopes = from scope in (new ScopeVisitor(parser)).Visit(fileUnitA)
                         let typeDefinition = (scope as TypeDefinition)
                         where typeDefinition != null
                         select typeDefinition;

            inventory.AddNewDefinitions(scopes);

            scopes = from scope in (new ScopeVisitor(parser)).Visit(fileUnitB)
                     let typeDefinition = (scope as TypeDefinition)
                     where typeDefinition != null
                     select typeDefinition;

            inventory.AddNewDefinitions(scopes);

            var testTypeUse = parser.CreateTypeUse(fileUnitC.Descendants(SRC.Type).First(), fileUnitC);

            var typeA = inventory.ResolveType(testTypeUse).FirstOrDefault();
            Assert.AreEqual("A", typeA.Name);

            var typeB = inventory.ResolveType(typeA.Parents.First()).FirstOrDefault();
            Assert.AreEqual("B", typeB.Name);
        }
    }
}
