grammar RecloserGrammar;

program: statement* EOF;


structDefinition: STRUCT structname = ID LCRLPAREN (structField)* RCRLPAREN SEMICOLON;

structField: type = typeRule name = ID SEMICOLON;

variableDeclaration: type = typeRule variable (COMMA variable)* SEMICOLON;

variable: name=ID (ASSIGN value=expression)?;

functionDeclaration: type = typeRule name=ID LPAREN functionSignature RPAREN SEMICOLON;

functionDefinition: type = typeRule name=ID LPAREN functionSignature RPAREN LCRLPAREN functionBody RCRLPAREN;

functionSignature: signatureArgument (COMMA signatureArgument) | ;

signatureArgument: way=(IN|INOUT)? type=typeRule name=ID;

functionBody: (statement)*;

typeRule: typename=ID (LSQRPAREN size=INT RSQRPAREN)?;

constant: value = INT #IntegerConstant
|value = FLOAT #FloatConstant
|value = (TRUE|FALSE) #BooleanConstant
;

statement: variableDeclaration #VariableDeclarationStatement
| expression SEMICOLON #ExpressionStatement
| LCRLPAREN (statement)* RCRLPAREN #ScopeStatement
| BREAK SEMICOLON #BreakStatement
| CONTINUE SEMICOLON #ContinueStatement
| WHILE LPAREN condition = expression RPAREN body = statement #WhileStatement
| SWITCH LPAREN expression RPAREN LCRLPAREN (case)* RCRLPAREN #SwitchStatement
| IF LPAREN condition = expression RPAREN ifbody = statement (ELSE elsebody = statement| {_input.La(1) != ELSE}?) #IfStatement
| RETURN expression? SEMICOLON #ReturnStatement
|functionDefinition #FunctionDefinitionStatement
|functionDeclaration #FunctionDeclarationStatement
|structDefinition #StructDefinitionStatement
;

case: (CASE constant|DEFAULT) COLON (statement)*;

expression:  LPAREN expression RPAREN #BracketExpression
| (INCREMENT|DECREMENT) expression #PreIncrementDecrement
| expression (INCREMENT|DECREMENT) #PostIncrementDecrement
| expression EQUAL expression #BinaryOperator
| expression AND expression #BinaryOperator
| expression OR expression #BinaryOperator
| NOT expression #UnaryOperator
| MINUS expression #UnaryOperator
| condition=expression QUESTIONMARK first=expression COLON second=expression #TernaryOperator
| left=expression (PLUSASSIGN|MINUSASSIGN|MULTIPLYASSIGN|DIVIDEASSIGN) right=expression #BinaryOperator
| left=expression (PLUS|MINUS) right=expression #BinaryOperator
| left=expression(ASTERISK|DIVISION) right=expression #BinaryOperator
| parent=expression DOT field = ID #FieldAccess
| left=expression ASSIGN right=expression #BinaryOperator
| name=ID LPAREN arguments RPAREN #FunctionCall
| array = expression LSQRPAREN index = expression RSQRPAREN #ArrayElementAccess
| id = ID #IdentifierExpression
| constant #ConstantExpression
| NEW typeID = ID LSQRPAREN size=INT RSQRPAREN LCRLPAREN (expression (COMMA expression)*)? RCRLPAREN #ArrayInitializerList
| NEW structName = ID LCRLPAREN (expression (COMMA expression))? RCRLPAREN #StructInitializerList
;

arguments: expression (COMMA expression)| ;

fragment LOWERCASE  : [a-z] ;
fragment UPPERCASE  : [A-Z] ;
fragment DIGIT: [0-9] ;

NEW: 'new';
SWITCH: 'switch';
ELSE: 'else';
RETURN: 'return';
IF: 'if';
WHILE: 'while';
CASE: 'case';
DEFAULT: 'default';
TRUE: 'true';
FALSE: 'false';
EQUAL: '==';
NOT: 'not'|'!';
OR: 'or'|'||';
AND: 'and'|'&&';
STRUCT: 'struct';
INOUT: 'inout';
IN: 'in';
BREAK: 'break';
CONTINUE: 'continue';

INT: DIGIT+ ; 
FLOAT: (DIGIT+ DOT DIGIT*) ([Ee][+-]? DIGIT+)?
	   |DOT DIGIT+ ([Ee][+-]? DIGIT+)?
		|DIGIT+ ([Ee] [+-]? DIGIT+)?
		;
ID		: [_]*(LOWERCASE|UPPERCASE)[A-Za-z0-9_]*;


MULTIPLYASSIGN		:'*=';
DIVIDEASSIGN		:'/=';
MINUSASSIGN			:'-=';
PLUSASSIGN			:'+=';
DECREMENT			:'--';
INCREMENT			:'++';
QUESTIONMARK		:'?' ;
PLUS               : '+' ;
MINUS              : '-' ;
ASTERISK           : '*' ;
DIVISION           : '/' ;
ASSIGN             : '=' ;
LPAREN             : '(' ;
RPAREN				: ')';
DOT					: '.';
COMMA				: ',' ;
SEMICOLON			: ';' ;
COLON				: ':' ;
ARRAYSIGN			:'[]';
LSQRPAREN			: '[' ;
RSQRPAREN			: ']' ;
LCRLPAREN			: '{' ;
RCRLPAREN			: '}' ;
ANGLE				:'@' ;
CARET				:'^';

STRING	: '"' .*? '"'|'\'' .*? '\'';
NEWLINE	: ('\r'? '\n' | '\r')+ -> skip;
WHITESPACE : (' ' | '\t')+ -> skip ;
COMMENT 
	:	( '//' ~[\r\n]* ('\r'? '\n' | 'r')
		| '/*' .*? '*/'
		) -> skip
	;