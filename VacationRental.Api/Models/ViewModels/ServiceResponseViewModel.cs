using System.Net;

namespace VacationRental.Api.Models
{
    public class ServiceResponseViewModel
    {
        public HttpStatusCode HttpCodeResponse { get; set; }
        public object Response { get; set; }
    }
}