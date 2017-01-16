using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.Target.x86_64
{
    class RegisterAllocator
    {
        public const int RegisterCount = 14;

        public enum RegisterAllocationType{
            None,
            Parameter,
            Local,
            Constant,
        };

        //TODO We can use rbp, but lets keep it available for now
        private int[] register_mapping;   //Maps indices to registers
        private RegisterAllocationType[] register_alloc_type;   //Maps register value types

        public RegisterAllocator()
        {
            register_mapping = new int[RegisterCount];
            register_alloc_type = new RegisterAllocationType[RegisterCount];
        }

        public int AllocateRegister(RegisterAllocationType type, int index)
        {
            for(int i = 0; i < RegisterCount; i++)
            {
                if(register_alloc_type[i] == RegisterAllocationType.None)
                {
                    register_alloc_type[i] = type;
                    register_mapping[i] = index;
                    return i;
                }
            }

            //Spillover, something needs to be moved out.
            return -1;
        }

        public void FreeRegister(int index)
        {
            register_alloc_type[index] = RegisterAllocationType.None;
        }

    }
}
