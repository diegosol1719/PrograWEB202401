using BackEnd.Models;
using BackEnd.Services.Interfaces;
using Entities.Entities;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadoController : ControllerBase
    {
        IEmpleadoService EmpleadoService;
        public EmpleadoController(IEmpleadoService empleadoService)
        {
            EmpleadoService = empleadoService;
        }

        // GET: api/<EmpleadoController>
        [HttpGet]
        public IEnumerable<EmpleadoModel> Get()
        {
            return EmpleadoService.GetEmpleados();
        }

        // GET api/<EmpleadoController>/5
        [HttpGet("{id}")]
        public EmpleadoModel Get(int id)
        {
            return EmpleadoService.GetById(id);
        }

        // POST api/<EmpleadoController>
        [HttpPost]
        public string Post([FromBody] EmpleadoModel empleado)
        {
            var result = EmpleadoService.AddEmpleado   (empleado);
            if (result) {
                return "Empleado agregado";
            }
            return "Empleado agregado";
    }

        // PUT api/<EmpleadoController>/5
        [HttpPut("{id}")]
        public string Put(int id, [FromBody] EmpleadoModel empleado)
        {
            var result = EmpleadoService.UpdateEmpleado(empleado);
            if (result)
            {
                return "Empleado Editado";
            }
            return "Empleado Editado";
        }

        // DELETE api/<EmpleadoController>/5
        [HttpDelete("{id}")]
        public string Delete(int id)
        
        {
            EmpleadoModel empleado = new EmpleadoModel { EmpleadoId = id };
            var result = EmpleadoService.DeleteEmpleado(empleado);
            if (result)
            {
                return "Empleado Eliminado";
            }
            return "Empleado Eliminado";
        }
    }
}
