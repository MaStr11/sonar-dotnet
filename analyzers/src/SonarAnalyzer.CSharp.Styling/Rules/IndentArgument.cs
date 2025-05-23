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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IndentArgument : IndentBase
{
    public IndentArgument() : base("T0029", "argument") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                if (ExpectedPosition(c.Node) is { } expected)
                {
                    Verify(c, expected, c.Node.GetFirstToken(), c.Node);
                    if (((ArgumentSyntax)c.Node).Expression is LambdaExpressionSyntax lambda && (lambda.Body ?? lambda.ExpressionBody) is { } expressionOrBody)
                    {
                        Verify(c, expected, expressionOrBody.GetFirstToken(), expressionOrBody);
                    }
                }
            },
            SyntaxKind.Argument);

    protected override SyntaxNode NodeRoot(SyntaxNode node, SyntaxNode current) =>
        current is ForStatementSyntax
            ? node.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault()
            : base.NodeRoot(node, current);
}
