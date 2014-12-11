using System;
using System.Collections.Generic;

namespace Lua.Compiler
{
    partial class FunctionContext
    {
        public void Function( string name = null )
        {
            if( !Compiler.Options.GenerateDebugInfo )
                return;

            Emit( new Instruction( InstructionType.Function, String( name ) ) );
        }

        public void Line( string fileName, int line )
        {
            if( !Compiler.Options.GenerateDebugInfo )
                return;

            Emit( new Instruction( InstructionType.Line, String( fileName ?? "<unknown>" ), new ImmediateOperand( line ) ) );
        }

        public int Bind( LabelOperand label )
        {
            Emit( new Instruction( InstructionType.Label, label ) );
            return 0;
        }

        public int LoadNil()
        {
            Emit( new Instruction( InstructionType.LdNil ) );
            return 1;
        }

        public int LoadTrue()
        {
            Emit( new Instruction( InstructionType.LdTrue ) );
            return 1;
        }

        public int LoadFalse()
        {
            Emit( new Instruction( InstructionType.LdFalse ) );
            return 1;
        }

        public int Load( IInstructionOperand operand )
        {
            if( operand is ConstantOperand<double> )
            {
                Emit( new Instruction( InstructionType.LdNum, operand ) );
                return 1;
            }

            if( operand is ConstantOperand<string> )
            {
                Emit( new Instruction( InstructionType.LdStr, operand ) );
                return 1;
            }

            var identifier = operand as IdentifierOperand;
            if( identifier != null )
            {
                if( identifier.FrameIndex != FrameIndex )
                    Emit( new Instruction( InstructionType.LdLoc, operand ) );
                else
                    Emit( new Instruction( InstructionType.LdLocF, new ImmediateOperand( identifier.Id ) ) );

                return 1;
            }

            throw new NotSupportedException();
        }

        public int LoadGlobal()
        {
            Emit( new Instruction( InstructionType.LdGlobal ) );
            return 1;
        }

        public int LoadField( ConstantOperand<string> operand )
        {
            Emit( new Instruction( InstructionType.LdFld, operand ) );
            return -1 + 1;
        }

        public int LoadArray()
        {
            Emit( new Instruction( InstructionType.LdArr ) );
            return -2 + 1;
        }

        public int Store( IdentifierOperand operand )
        {
            if( operand.FrameIndex != FrameIndex )
                Emit( new Instruction( InstructionType.StLoc, operand ) );
            else
                Emit( new Instruction( InstructionType.StLocF, new ImmediateOperand( operand.Id ) ) );

            return -1;
        }

        public int StoreField( ConstantOperand<string> operand )
        {
            Emit( new Instruction( InstructionType.StFld, operand ) );
            return -2;
        }

        public int StoreArray()
        {
            Emit( new Instruction( InstructionType.StArr ) );
            return -3;
        }

        public int NewObject( int length )
        {
            Emit( new Instruction( InstructionType.NewObject, new ImmediateOperand( length ) ) );
            return -length;
        }

        public int NewArray( int length )
        {
            Emit( new Instruction( InstructionType.NewArray, new ImmediateOperand( length ) ) );
            return -length + 1;
        }

        public int Dup()
        {
            Emit( new Instruction( InstructionType.Dup ) );
            return 1;
        }

        public int Drop()
        {
            Emit( new Instruction( InstructionType.Drop ) );
            return -1;
        }

        public int Swap()
        {
            Emit( new Instruction( InstructionType.Swap ) );
            return 0;
        }

        public int BinaryOperation( TokenType operation )
        {
            InstructionType type;
            if( !_binaryOperationMap.TryGetValue( operation, out type ) )
                throw new NotSupportedException();

            Emit( new Instruction( type ) );
            return -2 + 1;
        }

        public int UnaryOperation( TokenType operation )
        {
            InstructionType type;
            if( !_unaryOperationMap.TryGetValue( operation, out type ) )
                throw new NotSupportedException();

            Emit( new Instruction( type ) );
            return -1 + 1;
        }

        public int Closure( LabelOperand label )
        {
            Emit( new Instruction( InstructionType.Closure, label ) );
            return 1;
        }

        public int Call( int argumentCount )
        {
            Emit( new Instruction(
                InstructionType.Call,
                new ImmediateOperand( argumentCount ) ) );

            return -argumentCount - 1 + 1;
        }

        public int TailCall( int argumentCount, LabelOperand label )
        {
            Emit( new Instruction(
                InstructionType.TailCall,
                new ImmediateOperand( argumentCount ),
                label ) );

            return -argumentCount;
        }

        public int Return()
        {
            Emit( new Instruction( InstructionType.Ret ) );
            return -1;
        }

        public int Enter()
        {
            Emit( new Instruction( InstructionType.Enter ) );
            return 0;
        }

        public int VarArgs( int fixedCount )
        {
            Emit( new Instruction( InstructionType.VarArgs, new ImmediateOperand( fixedCount ) ) );
            return 0;
        }

        public int Jump( LabelOperand label )
        {
            Emit( new Instruction( InstructionType.Jmp, label ) );
            return 0;
        }

        public int JumpTrue( LabelOperand label )
        {
            Emit( new Instruction( InstructionType.JmpTrue, label ) );
            return -1;
        }

        public int JumpFalse( LabelOperand label )
        {
            Emit( new Instruction( InstructionType.JmpFalse, label ) );
            return -1;
        }

        public int JumpTruePeek( LabelOperand label )
        {
            Emit( new Instruction( InstructionType.JmpTrueP, label ) );
            return 0;
        }

        public int JumpFalsePeek( LabelOperand label )
        {
            Emit( new Instruction( InstructionType.JmpFalseP, label ) );
            return 0;
        }

        public int JumpTable( int start, List<LabelOperand> labels )
        {
            var startOp = new ImmediateOperand( start );
            var count = new DeferredImmediateOperand( () => labels.Count );
            var list = new ListOperand<LabelOperand>( labels );

            Emit( new Instruction( InstructionType.JmpTable, startOp, count, list ) );
            return -1;
        }

        private static Dictionary<TokenType, InstructionType> _binaryOperationMap;
        private static Dictionary<TokenType, InstructionType> _unaryOperationMap;

        static FunctionContext()
        {
            _binaryOperationMap = new Dictionary<TokenType, InstructionType>
            {
                { TokenType.Concat, InstructionType.Concat },

                { TokenType.Addition, InstructionType.Add },
                { TokenType.Subtraction, InstructionType.Sub },
                { TokenType.Multiplication, InstructionType.Mul },
                { TokenType.Division, InstructionType.Div },
                { TokenType.Modulo, InstructionType.Mod },
                { TokenType.Exponent, InstructionType.Exp },

                { TokenType.Equals, InstructionType.Eq },
                { TokenType.NotEquals, InstructionType.Neq },
                { TokenType.GreaterThan, InstructionType.Gt },
                { TokenType.GreaterThanOrEqual, InstructionType.Gte },
                { TokenType.LessThan, InstructionType.Lt },
                { TokenType.LessThanOrEqual, InstructionType.Lte },
            };

            _unaryOperationMap = new Dictionary<TokenType, InstructionType>
            {
                { TokenType.Subtraction, InstructionType.Neg },

                { TokenType.Not, InstructionType.Not }
            };
        }
    }
}
