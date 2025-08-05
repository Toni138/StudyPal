
    function confirmDelete(id) {
        Swal.fire({
            title: 'Are you sure?',
            text: "This flashcard will be deleted permanently.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                document.getElementById("flashcardIdToDelete").value = id;
                document.getElementById("deleteForm").submit();
            }
        });
    }
