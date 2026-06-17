"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { useGetOrganizations, useDeleteOrganization } from "@/hooks/api"
import { Organization } from "@/types"
import { Button } from "@/components/ui/shadcn/button"
import { Input } from "@/components/ui/shadcn/input"
import { Label } from "@/components/ui/shadcn/label"
import { Rocket } from "lucide-react"
import {
    Card,
    CardHeader,
    CardTitle,
    CardDescription,
    CardFooter,
} from "@/components/ui/shadcn/card"
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogDescription,
    DialogFooter,
} from "@/components/ui/shadcn/dialog"
import { PasswordInput } from "@/components/ui/inputs/PasswordInput"
import toast from "react-hot-toast";
import Main from "@/components/ui/Main";
import HeaderPlain from "@/components/ui/headers/HeaderPlain";
import {useOrgStore} from "@/stores/orgStore";

export default function Organizations() {
    const t = useTranslations('employer.organizations')
    const tCommon = useTranslations('common')
    const router = useRouter()
    const { data: organizations, isLoading, isError, refetch } = useGetOrganizations()
    const deleteOrganization = useDeleteOrganization()

    const { setCurrentOrg } = useOrgStore()
    const [deleteId, setDeleteId] = useState<number | null>(null)
    const [isDialogOpen, setIsDialogOpen] = useState(false)
    const [deletePassword, setDeletePassword] = useState("")

    const openDeleteDialog = (orgId: number) => {
        setDeleteId(orgId)
        setIsDialogOpen(true)
    }

    const handleDelete = () => {
        if (!deleteId) return
        deleteOrganization.mutate({ id: deleteId, password: deletePassword }, {
            onSuccess: () => {
                toast.success(t('deleted'))
                setIsDialogOpen(false)
                setDeletePassword("")
            },
            onError: () => {
                toast.error(t('failedToDelete'))
            },
        })
    }

    const handleNavigate = (orgId: number) => {
        localStorage.setItem("orgId", String(orgId))
        setCurrentOrg(orgId)
        router.push(`/dashboard`)
    }

    const handleEdit = (orgId: number) => {
        router.push(`organizations/${orgId}`)
    }

    const handleAdd = () => {
        router.push("organizations/create")
    }

    if (isLoading) return <p className="p-4">{t('loading')}</p>
    if (isError) return <p className="p-4 text-red-500">{t('failedToLoad')}</p>

    return (
        <>
            <HeaderPlain title={t('title')}/>
            <Main>
                {/* Onboarding CTA banner */}
                <div className="mb-6 flex items-center justify-between gap-4 rounded-lg border border-primary/20 bg-primary/5 px-4 py-3">
                    <div>
                        <p className="text-sm font-medium">{t('startOnboarding')}</p>
                        <p className="text-xs text-muted-foreground">{t('startOnboardingDesc')}</p>
                    </div>
                    <Button
                        variant="outline"
                        size="sm"
                        className="shrink-0"
                        onClick={() => router.push('/onboarding?from=organizations')}
                    >
                        <Rocket className="mr-2 h-4 w-4" />
                        {t('startOnboarding')}
                    </Button>
                </div>

                {organizations && organizations.length > 0 ? (
                    <>
                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                            {organizations.map((org: Organization) => (
                                <Card key={org.id} className="flex flex-col justify-between">
                                    <CardHeader>
                                        {org.photoUrl ? (
                                            <img
                                                src={org.photoUrl}
                                                alt={org.name}
                                                className="w-full h-40 object-cover rounded-md"
                                            />
                                        ) : (
                                            <div className="flex items-center justify-center h-40 rounded-md bg-muted">
                                                <span className="text-muted-foreground text-sm">{t('noImage')}</span>
                                            </div>
                                        )}
                                        <CardTitle className="mt-4">{org.name}</CardTitle>
                                        <CardDescription>{org.description || t('noDescription')}</CardDescription>
                                    </CardHeader>

                                    <CardFooter className="flex justify-between">
                                        <div className="flex gap-2">
                                            <Button size="sm" variant="default" onClick={() => handleNavigate(org.id)}>
                                                {t('goTo')}
                                            </Button>
                                            <Button size="sm" variant="secondary" onClick={() => handleEdit(org.id)}>
                                                {tCommon('edit')}
                                            </Button>
                                            <Button size="sm" variant="destructive"
                                                    onClick={() => openDeleteDialog(org.id)}>
                                                {tCommon('delete')}
                                            </Button>
                                        </div>
                                    </CardFooter>
                                </Card>
                            ))}
                        </div>

                        <Button onClick={handleAdd} className="fixed bottom-6 right-6 rounded-full w-14 h-14 text-2xl">
                            +
                        </Button>
                    </>
                ) : (
                    <div className="h-full text-center pt-40 space-y-4">
                        <p className="text-lg">{t('noOrganizationsYet')}</p>
                        <Button onClick={handleAdd}>{t('createFirstOrganization')}</Button>
                    </div>
                )}

                {/* Delete Dialog */}
                <Dialog open={isDialogOpen} onOpenChange={(open) => {
                    setIsDialogOpen(open)
                    if (!open) setDeletePassword("")
                }}>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>{t('deleteOrganization')}</DialogTitle>
                            <DialogDescription>
                                {t('deleteWarning')}
                            </DialogDescription>
                        </DialogHeader>
                        <div className="grid gap-1.5 py-2">
                            <Label htmlFor="orgDeletePassword">{t('deletePasswordLabel')}</Label>
                            <PasswordInput
                                id="orgDeletePassword"
                                value={deletePassword}
                                onChange={(e) => setDeletePassword(e.target.value)}
                            />
                        </div>
                        <DialogFooter>
                            <Button variant="secondary" onClick={() => setIsDialogOpen(false)}>
                                {tCommon('cancel')}
                            </Button>
                            <Button
                                variant="destructive"
                                onClick={handleDelete}
                                disabled={!deletePassword || deleteOrganization.isPending}
                            >
                                {tCommon('delete')}
                            </Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>
            </Main>
        </>
    )
}
