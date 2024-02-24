using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Impletations
{
    public class DALGenericoImpl <TEntity> : IDALGenerico<TEntity> where TEntity : class
    {

    }
}
