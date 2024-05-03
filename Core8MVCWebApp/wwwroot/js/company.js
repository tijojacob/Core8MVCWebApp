
var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable()
{
    dataTable = $('#myCompanyTable').DataTable({
        "ajax": { url: '/admin/company/getallcompany'},
        "columns": [
            { data: 'name', "width" : "12.5%" },
            { data: 'streetAddress', "width": "35%" },
            { data: 'city', "width": "10%" },
            { data: 'state', "width": "2.5%" },
            { data: 'postalCode', "width": "10%" },
            { data: 'phoneNumber', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="btn-group" role="group">
                            <a href="/admin/company/UpsertCompany?id=${data}" class="btn btn-primary">
                                <i class="bi bi-pencil-square"></i>Edit
                            </a>
                            <a onClick=Delete("/admin/company/delete?id=${data}") class="btn btn-danger mx-2 border">
                                <i class="bi bi-trash"></i>Delete
                            </a>
                        </div>`
                }, "width": "20%"
            }
        ]
    });

}

function Delete(Url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            //Swal.fire({
            //    title: "Deleted!",
            //    text: "Your file has been deleted.",
            //    icon: "success"
            //});
            $.ajax({
                url: Url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr["success"](data.message, "Success");
                }
            })
        }
    });
}