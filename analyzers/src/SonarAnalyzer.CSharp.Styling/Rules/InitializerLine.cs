﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InitializerLine : StylingAnalyzer
{
    private const int MaxLineLength = 200;  // According to S103

    public InitializerLine() : base("T0030", "Move this initializer to the previous line.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                foreach (var declarator in ((FieldDeclarationSyntax)c.Node).Declaration.Variables.Where(x => x.Initializer is not null))
                {
                    var expectedLine = declarator.GetLocation().StartLine();
                    if (MissplacedLocation(declarator.Initializer.Value, expectedLine) is { } location
                        && c.Node.SyntaxTree.GetText().Lines[expectedLine].Span.Length + location.SourceSpan.Length + 1 < MaxLineLength)
                    {
                        c.ReportIssue(Rule, location);
                    }
                }
            },
            SyntaxKind.FieldDeclaration);

    private static Location MissplacedLocation(ExpressionSyntax value, int expectedLine)
    {
        var valueLocation = value.GetLocation();
        if (valueLocation.StartLine() == expectedLine)
        {
            return null;
        }
        else if (value is ObjectCreationExpressionSyntax objectCreation)
        {
            return Location.Create(value.SyntaxTree, TextSpan.FromBounds(objectCreation.NewKeyword.SpanStart, objectCreation.ArgumentList?.Span.End ?? objectCreation.Type.Span.End));
        }
        else if (value is ImplicitObjectCreationExpressionSyntax implicitObjectCreation)
        {
            return Location.Create(value.SyntaxTree, TextSpan.FromBounds(implicitObjectCreation.NewKeyword.SpanStart, implicitObjectCreation.ArgumentList.Span.End));
        }
        else if (valueLocation.StartLine() != valueLocation.EndLine())
        {
            return null;
        }
        else
        {
            return value.GetLocation();
        }
    }
}
