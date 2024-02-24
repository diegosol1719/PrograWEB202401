using DAL.Interfaces;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Implementations
{
   
        public class UnidadDeTrabajo
        {
        public IEmpleadoDAL _empleadoDAL { get; }

        private readonly QuizContext _quizContext;
        public UnidadDeTrabajo(QuizContext quizContext, 
                                    IEmpleadoDAL empleadoDAL
            )
        {
            _quizContext =quizContext;
            _empleadoDAL =empleadoDAL;
        }

public bool Complete()
            {
                try
                {
                _quizContext.SaveChanges();
                    return true;
                }
                catch (Exception)
                {

                    return false;
                }
            }

            public void Dispose()
            {
            _quizContext.Dispose();
            }
        }
    }

