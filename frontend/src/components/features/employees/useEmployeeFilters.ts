import { useMemo, useState } from "react"
import { Employee } from "@/types"
import { EmployeeFiltersState, emptyFilters } from "./EmployeeFilters"

export function useEmployeeFilters(employees: Employee[]) {
    const [filters, setFilters] = useState<EmployeeFiltersState>(emptyFilters)

    const filteredEmployees = useMemo(() => {
        return employees.filter((employee) => {
            // Search filter
            if (filters.search) {
                const searchLower = filters.search.toLowerCase()
                const fullName = `${employee.firstName} ${employee.lastName}`.toLowerCase()
                const matchesSearch =
                    fullName.includes(searchLower) ||
                    employee.email.toLowerCase().includes(searchLower)

                if (!matchesSearch) return false
            }

            // Departments filter
            if (filters.departmentIds.length > 0) {
                const employeeDepartmentIds = employee.departmentIds ?? []
                const hasMatchingDepartment = filters.departmentIds.some((departmentId) =>
                    employeeDepartmentIds.includes(departmentId)
                )
                if (!hasMatchingDepartment) return false
            }

            return true
        })
    }, [employees, filters])

    return {
        filters,
        setFilters,
        filteredEmployees,
        totalCount: employees.length,
        filteredCount: filteredEmployees.length,
    }
}
