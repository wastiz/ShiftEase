# CSV Importer Component

## Поддерживаемые форматы CSV

CSVImporter автоматически распознает различные варианты названий колонок и форматы данных.

### Варианты названий колонок

#### Имя (First Name)
- `firstname`, `first name`, `first_name`
- `fname`, `givenname`, `given name`

#### Фамилия (Last Name)
- `lastname`, `last name`, `last_name`
- `lname`, `surname`, `familyname`, `family name`

#### Полное имя (Full Name)
- `name`, `fullname`, `full name`, `full_name`
- `employee name`, `employeename`

Если есть колонка с полным именем, компонент автоматически разделит его на имя и фамилию по пробелу.

#### Email
- `email`, `e-mail`, `mail`
- `email address`, `emailaddress`, `e_mail`

#### Телефон (Phone)
- `phone`, `telephone`, `mobile`
- `phonenumber`, `phone number`, `phone_number`
- `tel`, `contact`

#### Должность (Position)
- `position`, `role`, `title`
- `job`, `jobtitle`, `job title`, `job_title`

#### Группы (Departments)
- `department`, `departments`, `departmentid`, `department id`, `department_id`
- `departmentids`, `department ids`, `department_ids`
- `team`, `teams`

Поддерживается несколько групп через разделители: `,`, `;`, `|`
Например: `1,2,3` или `1;2;3` или `1|2|3`

#### Почасовая ставка (Hourly Rate)
- `hourly rate`, `hourlyrate`, `hourly_rate`
- `rate`, `wage`, `salary`, `pay`
- `payrate`, `pay rate`

#### Приоритет (Priority)
- `priority`, `importance`, `level`, `rank`

Значения автоматически нормализуются:
- `high`, `h`, `3`, `important`, `urgent` → `high`
- `low`, `l`, `1`, `minor` → `low`
- Остальные → `medium`

## Примеры CSV файлов

### Пример 1: Стандартный формат
```csv
First Name,Last Name,Email,Phone,Position,Department IDs,Hourly Rate,Priority
John,Doe,john@example.com,+1234567890,Manager,"1,2",25,high
Jane,Smith,jane@example.com,+0987654321,Cashier,1,18,medium
```

### Пример 2: С полным именем
```csv
Full Name,Email,Phone,Position,Teams,Rate,Importance
John Doe,john@example.com,+1234567890,Manager,1;2,25,high
Jane Smith,jane@example.com,+0987654321,Cashier,1,18,medium
```

### Пример 3: Альтернативные названия
```csv
name,e-mail,telephone,role,department,wage,level
John Doe,john@example.com,+1234567890,Manager,1|2,25,3
Jane Smith,jane@example.com,+0987654321,Cashier,1,18,2
```

### Пример 4: Минимальный формат
```csv
Name,Email
John Doe,john@example.com
Jane Smith,jane@example.com
```

## Обязательные поля

- **Email** - единственное обязательное поле
- Остальные поля заполнятся значениями по умолчанию, если не указаны

## Особенности

1. **Автоматическое разделение имени**: Если указано только полное имя, оно будет разделено на имя и фамилию
2. **Гибкий парсинг**: Поддерживает кавычки для значений с запятыми
3. **Предпросмотр**: Показывает превью данных перед импортом
4. **Предупреждения**: Уведомляет о проблемах в данных
5. **Валидация**: Проверяет наличие обязательных полей

## Использование

```tsx
import CSVImporter from '@/components/bulk-import/CSVImporter';

<CSVImporter onImport={(employees) => {
  // Обработка импортированных данных
  form.setValue('employees', employees);
}} />
```
