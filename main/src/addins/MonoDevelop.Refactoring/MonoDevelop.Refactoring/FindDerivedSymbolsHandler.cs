//
// FindDerivedSymbolsHandler.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2013 Xamarin
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Refactoring;
using MonoDevelop.Ide;
using ICSharpCode.NRefactory.TypeSystem;
using MonoDevelop.Ide.TypeSystem;
using System.Collections.Generic;
using System.Threading;
using MonoDevelop.Projects;
using MonoDevelop.Ide.FindInFiles;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System.Linq;
using Mono.TextEditor;
using ICSharpCode.NRefactory.Semantics;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.Analysis;

namespace MonoDevelop.Refactoring
{
	public class FindDerivedSymbolsHandler 
	{
		Ide.Gui.Document doc;
		IMember entity;

		public FindDerivedSymbolsHandler (Ide.Gui.Document doc, IMember entity)
		{
			this.doc = doc;
			this.entity = entity;
		}

		public void Run ()
		{
			TypeGraph tg = new TypeGraph (doc.Compilation.Assemblies);
			var node = tg.GetNode (entity.DeclaringTypeDefinition); 
			using (var monitor = IdeApp.Workbench.ProgressMonitors.GetSearchProgressMonitor (true, true)) {
				foreach (var derived in node.DerivedTypes) {
					var derivedMember = InheritanceHelper.GetDerivedMember (entity, derived.TypeDefinition);
					if (derivedMember == null)
						continue;
					var tf = TextFileProvider.Instance.GetReadOnlyTextEditorData (derivedMember.Region.FileName);
					var start = tf.LocationToOffset (derivedMember.Region.Begin); 
					tf.SearchRequest.SearchPattern = derivedMember.Name;
					var sr = tf.SearchForward (start); 
					if (sr != null) {
						start = sr.Offset;
					}

					monitor.ReportResult (new MemberReference (derivedMember, derivedMember.Region, start, derivedMember.Name.Length));
				}
			}
		}
	}
}
