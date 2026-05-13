using Oms.Core.Enums;
using Oms.Core.Entities;
using Oms.Infrastructure.Dtos.ExternalPriceMock;
using Oms.Core.Exceptions;

namespace Oms.Infrastructure.Mappers.ExternalPriceMock
{
    public static class AssetMapper
    {
        /// <summary>
        /// Convierte un AssetDto a un FinancialAsset del dominio. Se mapea cada propiedad y se convierte el tipo de activo utilizando el método MapAssetType.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static FinancialAsset ToDomain(this AssetResponse dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            
            return new FinancialAsset
            {
                Id = dto.Id,
                Ticker = dto.Ticker,
                Name = dto.Nombre,
                AssetType = MapAssetType(dto.TipoActivo),
                BasePrice = dto.PrecioUnitario
            };
        }

        /// <summary>
        /// Convierte un entero que representa el tipo de activo a un enum AssetType del dominio. Si el tipo no es reconocido, lanza una excepción ExternalServiceException.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ExternalServiceException"></exception>
        private static AssetType MapAssetType(int type)
        {
            return type switch
            {
                1 => AssetType.Accion,
                2 => AssetType.Bono,
                3 => AssetType.FCI,
                _ => throw new ExternalServiceException($"Tipo de activo desconocido: {type}")
            };
        }
    }
}