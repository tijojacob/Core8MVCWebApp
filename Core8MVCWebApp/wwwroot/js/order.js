
var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable()
{
    dataTable = $('#myOrderTable').DataTable({
        "ajax": { url: '/admin/order/GetAllOrders'},
        "columns": [
            { data: 'id', "width" : "12.5%" },
            { data: 'name', "width": "20%" },
            { data: 'phoneNumber', "width": "10%" },
            { data: 'applicationUser.email', "width": "12.5%" },
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="btn-group" role="group">
                            <a href="/admin/order/details?orderId=${data}" class="btn btn-primary">
                                <i class="bi bi-pencil-square"></i>
                            </a>                            
                        </div>`
                }, "width": "20%"
            }
        ]
    });

}