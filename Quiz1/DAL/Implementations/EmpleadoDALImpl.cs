using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Implementations
{
    public class EmpleadoDALImpl : DALGenericoImpl<Empleado>
    {
           QuizContext _quizContext;
        public EmpleadoDALImpl(QuizContext quiz) : base(quiz)
        {
            _quizContext = quiz;
        }

    }
}
