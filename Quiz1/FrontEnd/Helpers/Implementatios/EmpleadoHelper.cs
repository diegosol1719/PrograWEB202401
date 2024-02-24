using FrontEnd.ApiModels;
using FrontEnd.Helpers.Implementations;
using FrontEnd.Helpers.Interfaces;
using FrontEnd.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FrontEnd.Helpers.Implementatios
{
    public class EmpleadoHelper : IEmpleadoHelper
    {
        EmpleadoViewModel Convertir (Empleado empleado)
        {
            return new EmpleadoViewModel
                {
                EmpleadoId = empleado.EmpleadoId,
                Nombre = empleado.Nombre,
                Salario = empleado.Salario
            };
        }
        Empleado Convertir(EmpleadoViewModel empleado)
        {
            return new Empleado
            {
                EmpleadoId = empleado.EmpleadoId,
                Nombre = empleado.Nombre,
                Salario = empleado.Salario
            };
        }
        public EmpleadoViewModel AddEmpleado(EmpleadoViewModel empleado)
        {
            throw new NotImplementedException();
        }
        
        public EmpleadoViewModel DeleteEmpleado(int id)
        {
            throw new NotImplementedException();
        }

        public EmpleadoViewModel GetEmpleado(int id)
        {
            throw new NotImplementedException();
        }

        public List<EmpleadoViewModel> GetEmpleados()
        {
            HttpResponseMessage responseMessage = ServiceRepository.GetResponse("api/empleado");
            List<Empleado> resultado;
            if (responseMessage != null)
            {
                var context = responseMessage.Content.ReadAsStringAsync().Result;
                resultado = JsonConvert.DeserializeObject<List<Empleado>>(context);
            }
            List<EmpleadoViewModel> lista = new List<EmpleadoViewModel>();
            if (resultado!=null && resultado.Count>0)
          {
            }
            foreach (var item in resultado
                ) {
                lista.Add(Convertir(item));
            }
            return lista;

        }

        public EmpleadoViewModel UpdateEmpleado(EmpleadoViewModel empleado)
        {
            throw new NotImplementedException();
        }
    }
}
