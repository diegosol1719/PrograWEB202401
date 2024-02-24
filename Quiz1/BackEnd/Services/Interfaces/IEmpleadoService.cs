using BackEnd.Models;

namespace BackEnd.Services.Interfaces
{
    public interface IEmpleadoService
    {
      
            IEnumerable<EmpleadoModel> GetEmpleados();
            EmpleadoModel GetById(int id);
            bool AddEmpleado(EmpleadoModel empleado);
            bool UpdateEmpleado(EmpleadoModel empleado);
            bool DeleteEmpleado(EmpleadoModel empleado);
      

    }
}
