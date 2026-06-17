import { useMemo } from "react"
import { useTranslations } from "next-intl"
import { Search, X, Filter } from "lucide-react"
import { Input } from "@/components/ui/shadcn/input"
import { Badge } from "@/components/ui/shadcn/badge"
import { Button } from "@/components/ui/shadcn/button"
import { Checkbox } from "@/components/ui/shadcn/checkbox"
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/shadcn/popover"
import { Department } from "@/types"

export interface EmployeeFiltersState {
    search: string
    departmentIds: number[]
}

interface EmployeeFiltersProps {
    filters: EmployeeFiltersState
    onFiltersChange: (filters: EmployeeFiltersState) => void
    departments: Department[]
    totalCount: number
    filteredCount: number
}

export function EmployeeFilters({
    filters,
    onFiltersChange,
    departments,
    totalCount,
    filteredCount,
}: EmployeeFiltersProps) {
    const t = useTranslations('employer.employees')
    const tCommon = useTranslations('common')
    const hasActiveFilters = filters.search || filters.departmentIds.length > 0

    const selectedDepartments = useMemo(
        () => departments.filter((g) => filters.departmentIds.includes(g.id)),
        [departments, filters.departmentIds]
    )

    const updateSearch = (search: string) => {
        onFiltersChange({ ...filters, search })
    }

    const toggleDepartment = (departmentId: number) => {
        const newDepartmentIds = filters.departmentIds.includes(departmentId)
            ? filters.departmentIds.filter((id) => id !== departmentId)
            : [...filters.departmentIds, departmentId]
        onFiltersChange({ ...filters, departmentIds: newDepartmentIds })
    }

    const removeDepartment = (departmentId: number) => {
        onFiltersChange({
            ...filters,
            departmentIds: filters.departmentIds.filter((id) => id !== departmentId),
        })
    }

    const clearFilters = () => {
        onFiltersChange({ search: "", departmentIds: [] })
    }

    return (
        <div className="space-y-3">
            <div className="flex flex-col sm:flex-row gap-3">
                {/* Search input */}
                <div className="relative flex-1">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                        placeholder={t('searchPlaceholder')}
                        value={filters.search}
                        onChange={(e) => updateSearch(e.target.value)}
                        className="pl-9"
                    />
                    {filters.search && (
                        <button
                            onClick={() => updateSearch("")}
                            className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                        >
                            <X className="h-4 w-4" />
                        </button>
                    )}
                </div>

                {/* Departments filter */}
                <Popover>
                    <PopoverTrigger asChild>
                        <Button variant="outline" className="gap-2">
                            <Filter className="h-4 w-4" />
                            {tCommon('filter')}
                            {filters.departmentIds.length > 0 && (
                                <Badge variant="secondary" className="ml-1 px-1.5">
                                    {filters.departmentIds.length}
                                </Badge>
                            )}
                        </Button>
                    </PopoverTrigger>
                    <PopoverContent className="w-64 p-3" align="end">
                        <div className="space-y-2">
                            <p className="text-sm font-medium">{t('filterByDepartment')}</p>
                            {departments.length === 0 ? (
                                <p className="text-sm text-muted-foreground">
                                    {t('noDepartmentsAvailable')}
                                </p>
                            ) : (
                                <div className="space-y-1 max-h-48 overflow-y-auto">
                                    {departments.map((department) => {
                                        const isSelected = filters.departmentIds.includes(department.id)
                                        return (
                                            <div
                                                key={department.id}
                                                className="flex items-center gap-2 p-2 rounded-md cursor-pointer hover:bg-accent"
                                                onClick={() => toggleDepartment(department.id)}
                                            >
                                                <Checkbox checked={isSelected} />
                                                <div
                                                    className="w-3 h-3 rounded-full"
                                                    style={{ backgroundColor: department.color }}
                                                />
                                                <span className="text-sm">{department.name}</span>
                                            </div>
                                        )
                                    })}
                                </div>
                            )}
                        </div>
                    </PopoverContent>
                </Popover>

                {/* Clear filters */}
                {hasActiveFilters && (
                    <Button variant="ghost" size="sm" onClick={clearFilters}>
                        {t('clearAll')}
                    </Button>
                )}
            </div>

            {/* Active filters display */}
            {(selectedDepartments.length > 0 || hasActiveFilters) && (
                <div className="flex flex-wrap items-center gap-2">
                    {selectedDepartments.map((department) => (
                        <Badge
                            key={department.id}
                            variant="secondary"
                            className="gap-1 pr-1"
                            style={{
                                backgroundColor: `${department.color}20`,
                                borderColor: department.color,
                            }}
                        >
                            {department.name}
                            <button
                                onClick={() => removeDepartment(department.id)}
                                className="ml-1 rounded-full hover:bg-black/10 p-0.5"
                            >
                                <X className="h-3 w-3" />
                            </button>
                        </Badge>
                    ))}

                    {hasActiveFilters && (
                        <span className="text-sm text-muted-foreground ml-2">
                            {t('showingOfEmployees', { filtered: filteredCount, total: totalCount })}
                        </span>
                    )}
                </div>
            )}
        </div>
    )
}

export const emptyFilters: EmployeeFiltersState = {
    search: "",
    departmentIds: [],
}
