'use client'
import { useState } from 'react'
import { StickyNote, Save, X } from 'lucide-react'
import { Button } from '@/components/ui/shadcn/button'
import { Textarea } from '@/components/ui/shadcn/textarea'
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from '@/components/ui/shadcn/popover'
import { useTranslations } from 'next-intl'

type NoteEditorProps = {
    note?: string
    onSave: (note: string) => void
    entityName?: string
    buttonClassName?: string
}

export default function NoteEditor({ note, onSave, entityName, buttonClassName }: NoteEditorProps) {
    const t = useTranslations('notes')
    const [isOpen, setIsOpen] = useState(false)
    const [currentNote, setCurrentNote] = useState('')

    const handleOpen = (open: boolean) => {
        if (open) {
            setCurrentNote(note || '')
        }
        setIsOpen(open)
    }

    const handleSave = () => {
        onSave(currentNote)
        setIsOpen(false)
    }

    const handleCancel = () => {
        setCurrentNote(note || '')
        setIsOpen(false)
    }

    const hasNote = note && note.trim().length > 0

    return (
        <Popover open={isOpen} onOpenChange={handleOpen}>
            <PopoverTrigger asChild>
                <Button
                    variant="ghost"
                    size="icon"
                    className={`${buttonClassName} ${hasNote ? 'text-primary' : ''}`}
                    title={hasNote ? t('viewEditNote') : t('addNote')}
                >
                    <StickyNote className="h-3 w-3" />
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-80">
                <div className="space-y-3">
                    <div className="flex items-center justify-between">
                        <h4 className="font-medium text-sm">
                            {entityName ? t('noteFor', { name: entityName }) : t('note')}
                        </h4>
                    </div>

                    <Textarea
                        placeholder={t('placeholder')}
                        value={currentNote}
                        onChange={(e) => setCurrentNote(e.target.value)}
                        className="min-h-[100px] resize-none"
                        autoFocus
                    />

                    <div className="flex items-center justify-between text-xs text-muted-foreground">
                        <span>{currentNote.length} {t('characters')}</span>
                        {currentNote.length > 500 && (
                            <span className="text-orange-500">{t('longNote')}</span>
                        )}
                    </div>

                    <div className="flex gap-2">
                        <Button
                            onClick={handleSave}
                            size="sm"
                            className="flex-1"
                        >
                            <Save className="h-3 w-3 mr-1" />
                            {t('save')}
                        </Button>
                        <Button
                            onClick={handleCancel}
                            variant="outline"
                            size="sm"
                            className="flex-1"
                        >
                            <X className="h-3 w-3 mr-1" />
                            {t('cancel')}
                        </Button>
                    </div>
                </div>
            </PopoverContent>
        </Popover>
    )
}