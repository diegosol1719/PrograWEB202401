using FrontEnd.Models;

namespace FrontEnd.Helpers.Interfaces
{
    public interface IEmpleadoHelper
    {
        List<EmpleadoViewModel> GetEmpleados();
        EmpleadoViewModel GetEmpleado (int id);
        EmpleadoViewModel AddEmpleado (EmpleadoViewModel empleado);
        EmpleadoViewModel DeleteEmpleado(int id);
        EmpleadoViewModel UpdateEmpleado (EmpleadoViewModel empleado);
    }
}
