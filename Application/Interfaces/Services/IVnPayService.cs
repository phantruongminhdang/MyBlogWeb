using Application.Interfaces.Services.VNPay.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
    }
}
