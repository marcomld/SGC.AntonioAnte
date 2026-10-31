window.mostrarConfirmacionLiquidacion = function (anio) {
    return Swal.fire({
        title: '¿Emitir Liquidación Oficial?',
        text: `Se generará una obligación fiscal INMUTABLE para el año ${anio}. Esta acción no se puede deshacer.`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#10b981', // Verde success
        cancelButtonColor: '#ef4444', // Rojo danger
        confirmButtonText: 'Sí, emitir obligación',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        return result.isConfirmed;
    });
};