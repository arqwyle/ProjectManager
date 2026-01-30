let allEmployees = [];

document.addEventListener('DOMContentLoaded', async function() {
    try {
        const response = await fetch('/api/employees');
        allEmployees = await response.json();
        populateSelects();
    } catch (error) {
        console.error('Failed to load employees:', error);
        alert('Error loading employee list');
    }
});

function populateSelects() {
    const directorSelect = document.getElementById('directorId');
    directorSelect.innerHTML = '<option value="">Select director...</option>';

    const employeeSelect = document.getElementById('employeesId');
    employeeSelect.innerHTML = '';

    allEmployees.forEach(emp => {
        const dirOption = document.createElement('option');
        dirOption.value = emp.id;
        dirOption.textContent = `${emp.firstName} ${emp.lastName}`;
        directorSelect.appendChild(dirOption);

        const empOption = document.createElement('option');
        empOption.value = emp.id;
        empOption.textContent = `${emp.firstName} ${emp.lastName}`;
        employeeSelect.appendChild(empOption);
    });

    if (employeeSelect.loadOptions) {
        employeeSelect.loadOptions();
    }
}

document.getElementById('projectForm').addEventListener('submit', async function(e) {
    e.preventDefault();

    const btn = document.getElementById('createProjectBtn');
    const originalText = btn.textContent;

    const directorId = document.getElementById('directorId').value;
    const employeeOptions = document.getElementById('employeesId').selectedOptions;
    const selectedEmployees = Array.from(employeeOptions).map(opt => opt.value);

    btn.disabled = true;
    btn.textContent = 'Creating...';

    try {
        const projectData = {
            name: document.getElementById('name').value,
            customerName: document.getElementById('customerName').value,
            executorName: document.getElementById('executorName').value,
            startTime: document.getElementById('startTime').value,
            endTime: document.getElementById('endTime').value,
            priority: parseInt(document.getElementById('priority').value),
            directorId: directorId,
            employeeIds: selectedEmployees
        };

        const projectResponse = await fetch('/api/projects', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(projectData)
        });

        if (!projectResponse.ok) {
            throw new Error(`Project creation failed: ${await projectResponse.text()}`);
        }

        const project = await projectResponse.json();

        if (uploadedFiles.length > 0) {
            const formData = new FormData();
            uploadedFiles.forEach(file => {
                formData.append('files', file);
            });

            const uploadResponse = await fetch(`/api/projects/${project.id}/documents`, {
                method: 'POST',
                body: formData
            });

            if (!uploadResponse.ok) {
                throw new Error(`File upload failed: ${await uploadResponse.text()}`);
            }
        }

        alert('Project created successfully!');
        this.reset();
        uploadedFiles = [];
        preview.innerHTML = '';

        const employeeSelect = document.getElementById('employeesId');
        Array.from(employeeSelect.options).forEach(option => {
            option.selected = false;
        });
        if (employeeSelect.loadOptions) {
            employeeSelect.loadOptions();
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Error: ' + error.message);
    } finally {
        btn.disabled = false;
        btn.textContent = originalText;
    }
});