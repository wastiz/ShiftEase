'use client';

import { useState } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
    Plus,
    Trash2,
    Copy,
    Download,
    Users,
    CheckCircle2,
    XCircle,
    Loader2,
    Check,
    ChevronsUpDown
} from 'lucide-react';
import { Button } from '@/components/ui/shadcn/button';
import { Input } from '@/components/ui/shadcn/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/shadcn/card';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/shadcn/form';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/shadcn/select';
import { Alert, AlertDescription } from '@/components/ui/shadcn/alert';
import { Badge } from '@/components/ui/shadcn/badge';
import { Separator } from '@/components/ui/shadcn/separator';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/shadcn/popover';
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem } from '@/components/ui/shadcn/command';
import { Checkbox } from '@/components/ui/shadcn/checkbox';
import { toast } from 'sonner';
import Header from '@/components/ui/headers/Header';
import Main from '@/components/ui/Main';
import { useBulkCreateEmployees, useGetDepartments } from '@/hooks/api';
import { BulkCreateResult } from '@/types';
import CSVImporter from '@/components/features/employees/bulk-import/CSVImporter';
import { useTranslations } from 'next-intl';
import { EmployeeData } from '@/components/features/employees/bulk-import/CSVImporter';

const employeeSchema = z.object({
    firstName: z.string().min(1, 'First name is required'),
    lastName: z.string().min(1, 'Last name is required'),
    email: z.string().email('Invalid email'),
    phone: z.string().min(1, 'Phone is required'),
    position: z.string().min(1, 'Position is required'),
    departmentIds: z.array(z.number()),
    primaryDepartmentId: z.number().nullable(),
    hourlyRate: z.coerce.number().min(0, 'Hourly rate must be positive'),
    priority: z.enum(['low', 'medium', 'high']),
});

const formSchema = z.object({
    employees: z.array(employeeSchema).min(1, 'Add at least one employee'),
});

type FormValues = z.infer<typeof formSchema>;
type BulkEmployee = z.infer<typeof employeeSchema>;

const emptyEmployee: BulkEmployee = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    position: '',
    departmentIds: [],
    primaryDepartmentId: null,
    hourlyRate: 0,
    priority: 'medium',
};

export default function BulkAddEmployees() {
    const t = useTranslations('employer.bulkAdd');
    const [showResults, setShowResults] = useState(false);
    const [results, setResults] = useState<BulkCreateResult | null>(null);

    const { data: departments } = useGetDepartments();
    const bulkCreate = useBulkCreateEmployees();

    const form = useForm<FormValues>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            employees: [emptyEmployee],
        },
    });

    const { fields, append, remove, insert } = useFieldArray({
        control: form.control,
        name: 'employees',
    });

    const addEmployee = () => {
        append(emptyEmployee);
    };

    const duplicateEmployee = (index: number) => {
        const employee = form.getValues(`employees.${index}`);
        insert(index + 1, { ...employee, email: '', phone: '' });
    };

    const applyTemplate = (template: 'junior' | 'senior' | 'partTime') => {
        const templates = {
            junior: { hourlyRate: 15, priority: 'low' as const, position: 'Junior Employee' },
            senior: { hourlyRate: 40, priority: 'high' as const, position: 'Senior Employee' },
            partTime: { hourlyRate: 12, priority: 'medium' as const, position: 'Part-Time Employee' },
        };

        const currentEmployees = form.getValues('employees');
        const updated = currentEmployees.map(emp => ({
            ...emp,
            ...templates[template],
        }));
        form.setValue('employees', updated);
        toast.success(t('templateApplied'));
    };

    const exportTemplate = () => {
        const csv = [
            '# Employee Import Template',
            '# Supported column names (case-insensitive):',
            '# - Name fields: "First Name", "Last Name", or "Full Name" (will be split automatically)',
            '# - Email: "Email", "E-mail", "Mail"',
            '# - Phone: "Phone", "Telephone", "Mobile"',
            '# - Position: "Position", "Role", "Job Title"',
            '# - Departments: "Departments", "Department IDs", "Teams" (comma/semicolon/pipe separated)',
            '# - Rate: "Hourly Rate", "Rate", "Wage"',
            '# - Priority: "Priority" (high/medium/low)',
            '',
            'First Name,Last Name,Email,Phone,Position,Departments,Hourly Rate,Priority',
            'John,Doe,john.doe@example.com,+1234567890,Manager,"1,2",25,high',
            'Jane,Smith,jane.smith@example.com,+0987654321,Senior Cashier,1,22,medium',
            'Bob,Johnson,bob.j@example.com,+1112223333,Cashier,,18,low',
            '',
            '# Alternative format with Full Name:',
            '# Full Name,Email,Phone,Position,Departments,Hourly Rate,Priority',
            '# Alice Williams,alice.w@example.com,+4445556666,Assistant Manager,"1,2",23,medium',
        ].join('\n');

        const blob = new Blob([csv], { type: 'text/csv' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'employee_import_template.csv';
        link.click();
        window.URL.revokeObjectURL(url);
    };

    const handleCSVImport = (employees: EmployeeData[]) => {
        form.setValue('employees', employees);
    };

    const validateDuplicates = (employees: typeof emptyEmployee[]) => {
        const errors: string[] = [];
        const seenNames = new Map<string, number[]>();
        const seenEmails = new Map<string, number[]>();
        const seenPhones = new Map<string, number[]>();

        employees.forEach((emp, index) => {
            const fullName = `${emp.firstName.trim().toLowerCase()} ${emp.lastName.trim().toLowerCase()}`;
            const email = emp.email.trim().toLowerCase();
            const phone = emp.phone.trim();

            if (!seenNames.has(fullName)) {
                seenNames.set(fullName, []);
            }
            seenNames.get(fullName)!.push(index + 1);

            if (!seenEmails.has(email)) {
                seenEmails.set(email, []);
            }
            seenEmails.get(email)!.push(index + 1);

            if (!seenPhones.has(phone)) {
                seenPhones.set(phone, []);
            }
            seenPhones.get(phone)!.push(index + 1);
        });

        seenNames.forEach((indices, name) => {
            if (indices.length > 1) {
                errors.push(`Duplicate name "${name}" found in rows: ${indices.join(', ')}`);
            }
        });

        seenEmails.forEach((indices, email) => {
            if (indices.length > 1) {
                errors.push(`Duplicate email "${email}" found in rows: ${indices.join(', ')}`);
            }
        });

        seenPhones.forEach((indices, phone) => {
            if (indices.length > 1) {
                errors.push(`Duplicate phone "${phone}" found in rows: ${indices.join(', ')}`);
            }
        });

        return errors;
    };

    const onSubmit = async (data: FormValues) => {
        const validationErrors = validateDuplicates(data.employees);

        if (validationErrors.length > 0) {
            validationErrors.forEach(error => toast.error(error));
            return;
        }

        bulkCreate.mutate(data.employees, {
            onSuccess: (response) => {
                console.log(response);
                setResults(response);
                setShowResults(true);

                if (response.data.successCount > 0) {
                    toast.success(t('successCount', { count: response.data.successCount }));
                }
                if (response.data.failedCount > 0) {
                    toast.error(t('failedCount', { count: response.data.failedCount }));
                }
            },
            onError: () => {
                toast.error(t('failedToCreate2'));
            },
        });
    };

    if (showResults && results) {
        return (
            <>
                <Header title={t('resultsTitle')} />
                <Main>
                    <div className="container max-w-4xl mx-auto p-4 md:p-6 space-y-6">
                        <Card>
                            <CardHeader>
                                <CardTitle>{t('importResults')}</CardTitle>
                                <CardDescription>
                                    {t('resultsSummary', { success: results.data.successCount, failed: results.data.failedCount })}
                                </CardDescription>
                            </CardHeader>
                            <CardContent className="space-y-6">
                                {results.data.successfulEmployees && results.data.successfulEmployees.length > 0 && (
                                    <div className="space-y-3">
                                        <h3 className="font-semibold text-green-600 flex items-center gap-2">
                                            <CheckCircle2 className="h-5 w-5" />
                                            {t('successfullyCreated', { count: results.data.successCount })}
                                        </h3>
                                        <div className="space-y-2">
                                            {results.data.successfulEmployees.map((emp, idx: number) => (
                                                <div key={idx} className="flex items-center gap-2 p-2 border rounded">
                                                    <Badge variant="default" className="bg-green-50">
                                                        {emp.firstName} {emp.lastName}
                                                    </Badge>
                                                    <span className="text-sm text-muted-foreground">{emp.email}</span>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                )}

                                {results.data.failedEmployees && results.data.failedEmployees.length > 0 && (
                                    <div className="space-y-3">
                                        <h3 className="font-semibold text-red-600 flex items-center gap-2">
                                            <XCircle className="h-5 w-5" />
                                            {t('failedToCreate', { count: results.data.failedCount })}
                                        </h3>
                                        <div className="space-y-2">
                                            {results.data.failedEmployees.map((emp, idx: number) => (
                                                <Alert key={idx} variant="destructive">
                                                    <AlertDescription>
                                                        <strong>{emp.firstName} {emp.lastName}</strong> ({emp.email})
                                                        <br />
                                                        <span className="text-sm">{emp.errorMessage}</span>
                                                    </AlertDescription>
                                                </Alert>
                                            ))}
                                        </div>
                                    </div>
                                )}

                                <div className="flex gap-2">
                                    <Button onClick={() => setShowResults(false)}>
                                        {t('addMoreEmployees')}
                                    </Button>
                                    <Button variant="outline" onClick={() => window.location.href = '/employees'}>
                                        {t('goToEmployees')}
                                    </Button>
                                </div>
                            </CardContent>
                        </Card>
                    </div>
                </Main>
            </>
        );
    }

    return (
        <>
            <Header title={t('title')}>
                <div className="flex gap-2">
                    <Button variant="outline" size="sm" onClick={exportTemplate}>
                        <Download className="mr-2 h-4 w-4" />
                        {t('downloadTemplate')}
                    </Button>
                    <CSVImporter onImport={handleCSVImport} />
                </div>
            </Header>
            <Main>
                <div className="container max-w-full mx-auto p-4 md:p-6">
                    <Form {...form}>
                        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                            <Card>
                                <CardHeader>
                                    <div className="flex items-center justify-between">
                                        <div>
                                            <CardTitle>{t('addMultipleEmployees')}</CardTitle>
                                            <CardDescription>
                                                {t('fillDetails')}
                                            </CardDescription>
                                        </div>
                                        <Badge variant="secondary">
                                            <Users className="mr-1 h-3 w-3" />
                                            {fields.length} {t('employees')}
                                        </Badge>
                                    </div>
                                </CardHeader>
                                <CardContent className="space-y-4">
                                    <div className="flex gap-2 flex-wrap">
                                        <Button type="button" variant="outline" size="sm" onClick={() => applyTemplate('junior')}>
                                            {t('applyJuniorTemplate')}
                                        </Button>
                                        <Button type="button" variant="outline" size="sm" onClick={() => applyTemplate('senior')}>
                                            {t('applySeniorTemplate')}
                                        </Button>
                                        <Button type="button" variant="outline" size="sm" onClick={() => applyTemplate('partTime')}>
                                            {t('applyPartTimeTemplate')}
                                        </Button>
                                    </div>

                                    <Separator />

                                    <div className="overflow-x-auto border rounded-lg">
                                        <table className="w-full border-collapse">
                                            <thead className="bg-muted/50 sticky top-0">
                                                <tr>
                                                    <th className="p-3 text-left text-xs font-medium whitespace-nowrap border-b">#</th>
                                                    <th className="p-3 text-left text-xs font-medium whitespace-nowrap border-b min-w-[150px]">{t('firstName')}</th>
                                                    <th className="p-3 text-left text-xs font-medium whitespace-nowrap border-b min-w-[150px]">{t('lastName')}</th>
                                                    <th className="p-3 text-left text-xs font-medium whitespace-nowrap border-b min-w-[200px]">{t('email')}</th>
                                                    <th className="p-3 text-left text-xs font-medium whitespace-nowrap border-b min-w-[150px]">{t('phone')}</th>
                                                    <th className="p-3 text-left text-xs font-medium whitespace-nowrap border-b min-w-[150px]">{t('position')}</th>
                                                    <th className="p-3 text-left text-xs font-medium whitespace-nowrap border-b min-w-[180px]">{t('departments')}</th>
                                                    <th className="p-3 text-left text-xs font-medium whitespace-nowrap border-b min-w-[120px]">{t('hourlyRate')}</th>
                                                    <th className="p-3 text-left text-xs font-medium whitespace-nowrap border-b min-w-[120px]">{t('priority')}</th>
                                                    <th className="p-3 text-left text-xs font-medium whitespace-nowrap border-b sticky right-0 bg-muted/50">{t('actions')}</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {fields.map((field, index) => (
                                                    <tr key={field.id} className="border-b hover:bg-muted/20">
                                                        <td className="p-3 text-sm text-muted-foreground whitespace-nowrap">{index + 1}</td>
                                                        <td className="p-3">
                                                            <FormField
                                                                control={form.control}
                                                                name={`employees.${index}.firstName`}
                                                                render={({ field }) => (
                                                                    <FormItem>
                                                                        <FormControl>
                                                                            <Input {...field} placeholder="John" className="h-9" />
                                                                        </FormControl>
                                                                        <FormMessage className="text-xs" />
                                                                    </FormItem>
                                                                )}
                                                            />
                                                        </td>
                                                        <td className="p-3">
                                                            <FormField
                                                                control={form.control}
                                                                name={`employees.${index}.lastName`}
                                                                render={({ field }) => (
                                                                    <FormItem>
                                                                        <FormControl>
                                                                            <Input {...field} placeholder="Doe" className="h-9" />
                                                                        </FormControl>
                                                                        <FormMessage className="text-xs" />
                                                                    </FormItem>
                                                                )}
                                                            />
                                                        </td>
                                                        <td className="p-3">
                                                            <FormField
                                                                control={form.control}
                                                                name={`employees.${index}.email`}
                                                                render={({ field }) => (
                                                                    <FormItem>
                                                                        <FormControl>
                                                                            <Input {...field} type="email" placeholder="john@example.com" className="h-9" />
                                                                        </FormControl>
                                                                        <FormMessage className="text-xs" />
                                                                    </FormItem>
                                                                )}
                                                            />
                                                        </td>
                                                        <td className="p-3">
                                                            <FormField
                                                                control={form.control}
                                                                name={`employees.${index}.phone`}
                                                                render={({ field }) => (
                                                                    <FormItem>
                                                                        <FormControl>
                                                                            <Input {...field} placeholder="+1234567890" className="h-9" />
                                                                        </FormControl>
                                                                        <FormMessage className="text-xs" />
                                                                    </FormItem>
                                                                )}
                                                            />
                                                        </td>
                                                        <td className="p-3">
                                                            <FormField
                                                                control={form.control}
                                                                name={`employees.${index}.position`}
                                                                render={({ field }) => (
                                                                    <FormItem>
                                                                        <FormControl>
                                                                            <Input {...field} placeholder="Manager" className="h-9" />
                                                                        </FormControl>
                                                                        <FormMessage className="text-xs" />
                                                                    </FormItem>
                                                                )}
                                                            />
                                                        </td>
                                                        <td className="p-3">
                                                            <FormField
                                                                control={form.control}
                                                                name={`employees.${index}.departmentIds`}
                                                                render={({ field }) => (
                                                                    <FormItem>
                                                                        <Popover>
                                                                            <PopoverTrigger asChild>
                                                                                <FormControl>
                                                                                    <Button
                                                                                        variant="outline"
                                                                                        role="combobox"
                                                                                        className="h-9 w-full justify-between text-xs"
                                                                                    >
                                                                                        {field.value && field.value.length > 0
                                                                                            ? t('departmentCount', { count: field.value.length })
                                                                                            : t('selectDepartments')}
                                                                                        <ChevronsUpDown className="ml-2 h-3 w-3 shrink-0 opacity-50" />
                                                                                    </Button>
                                                                                </FormControl>
                                                                            </PopoverTrigger>
                                                                            <PopoverContent className="w-[200px] p-0">
                                                                                <Command>
                                                                                    <CommandInput placeholder={t('searchDepartments')} />
                                                                                    <CommandEmpty>{t('noDepartmentFound')}</CommandEmpty>
                                                                                    <CommandGroup className="max-h-64 overflow-auto">
                                                                                        {departments?.map((department) => {
                                                                                            const isSelected = field.value?.includes(department.id);
                                                                                            return (
                                                                                                <CommandItem
                                                                                                    key={department.id}
                                                                                                    onSelect={() => {
                                                                                                        const currentValue = field.value || [];
                                                                                                        let newValue: number[];
                                                                                                        if (isSelected) {
                                                                                                            newValue = currentValue.filter((id: number) => id !== department.id);
                                                                                                        } else {
                                                                                                            newValue = [...currentValue, department.id];
                                                                                                        }
                                                                                                        field.onChange(newValue);

                                                                                                        // Clear primaryDepartmentId if it's no longer in selected departments
                                                                                                        const currentPrimaryId = form.getValues(`employees.${index}.primaryDepartmentId`);
                                                                                                        if (currentPrimaryId && !newValue.includes(currentPrimaryId)) {
                                                                                                            form.setValue(`employees.${index}.primaryDepartmentId`, null);
                                                                                                        }
                                                                                                    }}
                                                                                                >
                                                                                                    <div className="flex items-center gap-2">
                                                                                                        <Checkbox
                                                                                                            checked={isSelected}
                                                                                                            className="h-4 w-4"
                                                                                                        />
                                                                                                        <span className="text-sm">{department.name}</span>
                                                                                                    </div>
                                                                                                </CommandItem>
                                                                                            );
                                                                                        })}
                                                                                    </CommandGroup>
                                                                                </Command>
                                                                            </PopoverContent>
                                                                        </Popover>
                                                                        <FormMessage className="text-xs" />
                                                                    </FormItem>
                                                                )}
                                                            />
                                                        </td>
                                                        <td className="p-3">
                                                            <FormField
                                                                control={form.control}
                                                                name={`employees.${index}.primaryDepartmentId`}
                                                                render={({ field }) => {
                                                                    const rowDepartmentIds = form.watch(`employees.${index}.departmentIds`) || [];
                                                                    return (
                                                                        <FormItem>
                                                                            <Select
                                                                                onValueChange={(val) => field.onChange(val === 'none' ? null : parseInt(val))}
                                                                                value={field.value?.toString() || 'none'}
                                                                                disabled={rowDepartmentIds.length === 0}
                                                                            >
                                                                                <FormControl>
                                                                                    <SelectTrigger className="h-9 text-xs">
                                                                                        <SelectValue placeholder={rowDepartmentIds.length === 0 ? "—" : t('selectPrimaryDepartment')} />
                                                                                    </SelectTrigger>
                                                                                </FormControl>
                                                                                <SelectContent>
                                                                                    <SelectItem value="none">None</SelectItem>
                                                                                    {rowDepartmentIds.map((id) => {
                                                                                        const department = departments?.find(g => g.id === id);
                                                                                        return department ? (
                                                                                            <SelectItem key={id} value={id.toString()}>
                                                                                                {department.name}
                                                                                            </SelectItem>
                                                                                        ) : null;
                                                                                    })}
                                                                                </SelectContent>
                                                                            </Select>
                                                                            <FormMessage className="text-xs" />
                                                                        </FormItem>
                                                                    );
                                                                }}
                                                            />
                                                        </td>
                                                        <td className="p-3">
                                                            <FormField
                                                                control={form.control}
                                                                name={`employees.${index}.hourlyRate`}
                                                                render={({ field }) => (
                                                                    <FormItem>
                                                                        <FormControl>
                                                                            <Input {...field} type="number" step="0.01" placeholder="25.00" className="h-9" />
                                                                        </FormControl>
                                                                        <FormMessage className="text-xs" />
                                                                    </FormItem>
                                                                )}
                                                            />
                                                        </td>
                                                        <td className="p-3">
                                                            <FormField
                                                                control={form.control}
                                                                name={`employees.${index}.priority`}
                                                                render={({ field }) => (
                                                                    <FormItem>
                                                                        <Select
                                                                            onValueChange={field.onChange}
                                                                            value={field.value}
                                                                        >
                                                                            <FormControl>
                                                                                <SelectTrigger className="h-9">
                                                                                    <SelectValue placeholder={t('selectPriority')} />
                                                                                </SelectTrigger>
                                                                            </FormControl>
                                                                            <SelectContent>
                                                                                <SelectItem value="low">{t('low')}</SelectItem>
                                                                                <SelectItem value="medium">{t('medium')}</SelectItem>
                                                                                <SelectItem value="high">{t('high')}</SelectItem>
                                                                            </SelectContent>
                                                                        </Select>
                                                                        <FormMessage className="text-xs" />
                                                                    </FormItem>
                                                                )}
                                                            />
                                                        </td>
                                                        <td className="p-3 sticky right-0 bg-background">
                                                            <div className="flex gap-1">
                                                                <Button
                                                                    type="button"
                                                                    variant="ghost"
                                                                    size="sm"
                                                                    onClick={() => duplicateEmployee(index)}
                                                                    className="h-8 w-8 p-0"
                                                                >
                                                                    <Copy className="h-3.5 w-3.5" />
                                                                </Button>
                                                                {fields.length > 1 && (
                                                                    <Button
                                                                        type="button"
                                                                        variant="ghost"
                                                                        size="sm"
                                                                        onClick={() => remove(index)}
                                                                        className="h-8 w-8 p-0"
                                                                    >
                                                                        <Trash2 className="h-3.5 w-3.5 text-destructive" />
                                                                    </Button>
                                                                )}
                                                            </div>
                                                        </td>
                                                    </tr>
                                                ))}
                                            </tbody>
                                        </table>
                                    </div>

                                    <Button
                                        type="button"
                                        variant="outline"
                                        className="w-full"
                                        onClick={addEmployee}
                                    >
                                        <Plus className="mr-2 h-4 w-4" />
                                        {t('addAnotherEmployee')}
                                    </Button>
                                </CardContent>
                            </Card>

                            <div className="flex justify-end gap-3">
                                <Button type="button" variant="outline" onClick={() => window.history.back()}>
                                    {t('cancel')}
                                </Button>
                                <Button type="submit" disabled={bulkCreate.isPending}>
                                    {bulkCreate.isPending ? (
                                        <>
                                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                            {t('creating')}
                                        </>
                                    ) : (
                                        <>
                                            <Users className="mr-2 h-4 w-4" />
                                            {t('createEmployees', { count: fields.length })}
                                        </>
                                    )}
                                </Button>
                            </div>
                        </form>
                    </Form>
                </div>
            </Main>
        </>
    );
}
